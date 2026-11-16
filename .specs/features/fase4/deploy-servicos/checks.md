# Deploy por serviço checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx`; `dotnet build submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx`; `dotnet build submodules/AutoReparos.Execucao/AutoReparos.Execucao.slnx`; `python3 scripts/specs/assert_deploy.py --check iac-formatado`
Plan: `.specs/features/fase4/deploy-servicos/plan.md`

35 checks · 6 slices · 9 one-way doors · 2 open questions, 2 block go-live (nenhuma bloqueia S1 a S5)

Derivados em 2026-11-15, depois da aprovação ("Plano está aprovado"). Regras de IaC, chart e workflow são provadas sem AWS (`terraform test` com `mock_provider`, `helm template`, asserts estáticos); as políticas IAM são avaliadas pelo simulador do IAM (`aws iam simulate-custom-policy`, perfil `autoreparos`, sem criar recursos); só o S6 exige o ambiente no ar.

## Checks

### S1 - Cada serviço sobe sozinho e diz se está saudável

**C1** - `docker build` em `AutoReparos.OS` gera imagem a partir do `Dockerfile` do repositório, com a porta `8080` exposta e usuário diferente de `root` (AC 1)
Proof: `python3 scripts/specs/assert_deploy.py --check dockerfile-os`

**C2** - No OS, `GET /health` responde `200` sem token com a database acessível e `503` com a database fora (AC 2)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Api.Tests --filter FullyQualifiedName~SaudeEndpointTests`

**C3** - Na Cobrança, `GET /health` responde `200` sem token com a database acessível e `503` com a database fora (AC 2)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~SaudeEndpointTests`

**C4** - Na Execução, `GET /health` responde `200` sem token com a database acessível e `503` com a database fora (AC 2)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~SaudeEndpointTests`

**C5** - No OS, `Database__RunMigrations=true` aplica todas as migrations numa database vazia e encerra com código `0` sem abrir HTTP; uma migration que falha encerra com código diferente de `0` (AC 3)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Api.Tests --filter FullyQualifiedName~MigrarESairTests`

**C6** - Na Cobrança, `Database__RunMigrations=true` aplica todas as migrations e encerra com `0` sem abrir HTTP; falha encerra com código diferente de `0` (AC 3)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~MigrarESairTests`

**C7** - Na Execução, `Database__RunMigrations=true` aplica todas as migrations e encerra com `0` sem abrir HTTP; falha encerra com código diferente de `0` (AC 3)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~MigrarESairTests`

**C8** - Na Cobrança, com `Database__SkipStartupMigration=true` o startup não cria tabelas; sem a flag o startup continua migrando (AC 4)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~MigracaoNaInicializacaoTests`

**C9** - Na Execução, com `Database__SkipStartupMigration=true` o startup não cria tabelas; sem a flag o startup continua migrando (AC 4)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~MigracaoNaInicializacaoTests`

### S2 - Os recursos AWS de cada serviço vêm do repositório dele

**C10** - `terraform validate` passa em `infra/terraform` de OS, Cobrança, Execução e Cadastros, e cada backend usa a chave `servicos/<servico>/production.tfstate` (AC 5)
Proof: `python3 scripts/specs/assert_deploy.py --check terraform-servicos`

**C11** - O Terraform de cada serviço declara o ECR `autoreparos-<servico>`, cada fila de entrada com a DLQ correspondente (`redrive_policy` com `maxReceiveCount`), a role IRSA presa ao ServiceAccount `<servico>` do namespace `<servico>` e o segredo `autoreparos/production/<servico>/database` (AC 6)
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.OS/infra/terraform recursos`
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.Cobranca/infra/terraform recursos`
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.Execucao/infra/terraform recursos`
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.Cadastros/infra/terraform recursos`

**C12** - O Terraform do OS declara `autoreparos-os-saga` e `autoreparos-os-saga-eventos` (esta com TTL) e a política IRSA do OS alcança só essas duas tabelas (AC 7)
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.OS/infra/terraform dynamodb`

**C13** - O simulador do IAM avalia as políticas IRSA: OS pode `sqs:SendMessage` nas filas de entrada de Cobrança, Execução e Cadastros e nas próprias, e é negado numa fila fora da lista; Cobrança só alcança a fila da Saga; Execução só alcança as filas do OS e da Cobrança e é negada na tabela `autoreparos-os-saga` (AC 8)
Proof: `python3 scripts/specs/simular_iam.py --caso irsa`

### S3 - O chart instala, migra e desfaz sozinho

**C14** - `helm template` de cada um dos 4 charts produz um Job de migration com hook `pre-install,pre-upgrade` e `hook-delete-policy: before-hook-creation,hook-succeeded`, e nenhum Job de criação de database (AC 9)
Proof: `python3 scripts/specs/assert_deploy.py --check chart-hooks`

**C15** - O Deployment de cada chart tem 1 réplica, HPA de 1 a 3, `readinessProbe` e `livenessProbe` em `/health` porta `8080`, ServiceAccount `<servico>` anotado com a role IRSA e as variáveis `AWS_REGION=us-east-1` e `Database__SkipStartupMigration=true` (AC 10)
Proof: `python3 scripts/specs/assert_deploy.py --check chart-deployment`

**C16** - O Ingress de cada chart usa `group.name: autoreparos-internal` com exatamente os prefixos da tabela de roteamento do plano, incluindo `/.well-known` no Cadastros, e o `group.order` do Cadastros é maior que os de OS, Cobrança e Execução (AC 11)
Proof: `python3 scripts/specs/assert_deploy.py --check chart-ingress`

**C17** - Nenhum objeto renderizado pelos 4 charts e nenhuma política da role de deploy referencia `autoreparos/production/database/credentials`; a connection string vem de um Secret do namespace do serviço (AC 13)
Proof: `python3 scripts/specs/assert_deploy.py --check sem-admin`

### S4 - Merge na `main` implanta o serviço

**C18** - O `ci-cd.yml` de OS, Cobrança, Execução e Cadastros tem o job `deploy` com `needs: [build, test, quality]`, condição `refs/heads/main`, `environment: production`, `id-token: write`, região `us-east-1`, chamada ao provisionador antes do Helm e `helm upgrade --install --atomic` (AC 14, AC 15)
Proof: `python3 scripts/specs/assert_pipelines.py --check deploy-producao`

**C19** - Depois do `helm upgrade`, o job `deploy` executa `kubectl rollout status` e um `GET /health` que falha o job quando a resposta não é `200` (AC 16)
Proof: `python3 scripts/specs/assert_pipelines.py --check deploy-verifica-saude`

**C20** - Em push na `develop` e em `pull_request`, nenhum passo do `ci-cd.yml` publica imagem nem implanta (AC 17)
Proof: `python3 scripts/specs/assert_pipelines.py --check sem-deploy-fora-da-main`

**C21** - A regra `deploy-staging` não existe mais em `assert_pipelines.py` (AC 18, AD-044)
Proof: `python3 scripts/specs/assert_deploy.py --check sem-staging`

### S5 - O pai fica só com a plataforma

**C22** - `stacks/base` tem `instance_types` com padrão `["t3.medium"]`, `desired` e `min` 2 e `max` 3 (AC 19)
Proof: `python3 scripts/specs/assert_deploy.py --check plataforma-nos`

**C23** - A plataforma cria os namespaces `os`, `cobranca`, `execucao` e `cadastros`, o boundary `autoreparos-servico-boundary` e as 4 roles `autoreparos-deploy-<servico>` com trust só em `repo:Grupo78-PosTech-15SOAT/AutoReparos.<Repo>:environment:production` e access entry `AmazonEKSEditPolicy` limitada ao namespace do serviço (AC 20)
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base roles-deploy`

**C24** - O simulador do IAM nega à role de deploy da Cobrança `sqs:CreateQueue` em `autoreparos-os-*` e `iam:CreateRole` sem o boundary, e permite `sqs:CreateQueue` em `autoreparos-cobranca-*` e `lambda:InvokeFunctionUrl` no provisionador (AC 20)
Proof: `python3 scripts/specs/simular_iam.py --caso deploy`

**C25** - `provision-aws.yml` e `deploy-app.yml` constroem e publicam só a imagem do Web, e o `helm upgrade` do guarda-chuva não contém o subchart `api` (AC 21)
Proof: `python3 scripts/specs/assert_deploy.py --check pai-so-web`

**C26** - O Ingress do Web no chart guarda-chuva usa o grupo `autoreparos-internal` com o maior `group.order` entre todos os Ingress (AC 22)
Proof: `python3 scripts/specs/assert_deploy.py --check web-ingress`

**C27** - A plataforma publica no SSM `/autoreparos/production/eks/cluster_name`, `/eks/oidc_provider_arn` e `/eks/oidc_provider_url`, e nenhum Terraform de serviço usa `terraform_remote_state` (AC 23)
Proof: `python3 scripts/specs/assert_deploy.py --check ssm-plataforma`

**C28** - A Lambda `autoreparos-provisionador-db` fica na VPC com acesso ao RDS, tem Function URL `AWS_IAM` e é a única role com `secretsmanager:GetSecretValue` na credencial admin do RDS (AC 24)
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.Database/terraform provisionador`

**C29** - Chamado pela role `autoreparos-deploy-<servico>`, o provisionador cria só `<servico>_db` e `<servico>_app` com a senha do segredo do serviço; numa segunda chamada responde sucesso sem recriar nem trocar a senha; chamada de outra role ou sem assinatura responde `403` sem tocar no banco; a role do OS nunca cria `cobranca_db` (AC 12)
Proof: `python3 -m unittest discover -s submodules/AutoReparos.Infra.Database/lambda/provisionador/tests -p test_provisionador.py`

**C30** - A VPC tem endpoints do tipo gateway de S3 e DynamoDB associados às tabelas de rota privadas e continua com exatamente um NAT Gateway (AC 25)
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base vpc-endpoints`

**C31** - Depois de `scripts/infra/configurar-github.sh`, o pai tem `AWS_ACCOUNT_ID=198903849859`, `TF_STATE_BUCKET=autoreparos-tfstate-198903849859` e `AWS_ROLE_ARN` de `autoreparos-github-pai` em `production` e `aws-bootstrap`; cada serviço tem `AWS_REGION`, `TF_STATE_BUCKET`, `TF_LOCK_TABLE` e o Environment `production` com revisores, só `main` e `AWS_ROLE_ARN` da própria role; a `CHAVE_INTERNA` do pai e do Cadastros vêm do mesmo parâmetro SSM (AC 26)
Proof: `scripts/infra/configurar-github.sh --verificar`

**C32** - Rodar `scripts/infra/configurar-github.sh` duas vezes não gera chaves novas: os parâmetros `SecureString` da chave JWT, da `CHAVE_INTERNA` e da senha do usuário inicial continuam na versão `1` (AC 26)
Proof: `scripts/infra/configurar-github.sh --verificar-idempotencia`

### S6 - Um serviço implantado de verdade

**C33** - O último workflow da `main` da Execução terminou verde depois da aprovação, e `GET <API Gateway>/api/v1/servicos` responde `200` com os 5 serviços semeados usando um token emitido pelo Cadastros do mesmo ambiente, e `401` sem token (AC 27)
Proof: `scripts/specs/smoke_deploy.sh execucao`

**C34** - Depois de implantar uma versão com `/health` quebrado, `helm history execucao` mostra essa revisão como `failed` e a anterior de novo como `deployed`, e `/api/v1/servicos` continua respondendo (AC 28)
Proof: `scripts/specs/assert_rollback.sh execucao`

**C35** - Os nomes da porta 4 batem entre produtores e consumidores: cada URL de fila que um serviço recebe do chart aponta para uma fila declarada no Terraform do consumidor (porta 4)
Proof: `python3 scripts/specs/assert_deploy.py --check filas-produtor-consumidor`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| `GET /health` statuses × serviços (6) | `200` OS C2 · Cobrança C3 · Execução C4 · `503` OS C2 · Cobrança C3 · Execução C4 | - |
| flag que migra e sai, AC 3 (6) | OS vazia→0 C5 · OS falha→≠0 C5 · Cobrança vazia→0 C6 · Cobrança falha→≠0 C6 · Execução vazia→0 C7 · Execução falha→≠0 C7 | - |
| startup config: migração no startup (4) | Cobrança `Program.cs` com flag C8 · Cobrança harness sem flag C8 · Execução `Program.cs` com flag C9 · Execução harness sem flag C9 | - |
| one-way doors (9) | 1 IaC no serviço C10 C11 · 2 role de deploy C23 C24 · 3 state C10 · 4 nomes e dono da fila C11 C35 · 5 chart C14 C15 · 6 namespace C15 C23 · 7 flag C5 C6 C7 · 8 grupo do ALB C16 C26 · 9 database e usuário C28 C29 | - |
| serviços com Terraform (4) | OS C10 C11 · Cobrança C10 C11 · Execução C10 C11 · Cadastros C10 C11 | - |
| serviços com chart (4) | C14 C15 C16 C17, table-driven sobre os 4 | - |
| serviços com job `deploy` (4) | C18 C19 C20, table-driven sobre os 4 | - |
| tabela de roteamento do plano (4 linhas) | OS C16 · Cobrança C16 · Execução C16 · Cadastros com `/.well-known` C16 · Web por último C26 | - |
| IRSA, AC 8 (4) | OS nas 3 filas C13 · OS fora da lista negado C13 · Cobrança só Saga C13 · Execução sem tabela C13 | - |
| role de deploy, AC 20 (4 exemplos) | CreateQueue `autoreparos-os-*` negado C24 · CreateRole sem boundary negado C24 · própria fila permitida C24 · provisionador permitido C24 | - |
| provisionador, AC 12 (4 exemplos) | nova C29 · existente C29 · outra role 403 C29 · OS sem `cobranca_db` C29 | - |
| configuração do GitHub, AC 26 (11) | pai `AWS_ACCOUNT_ID` C31 · pai `TF_STATE_BUCKET` C31 · pai `AWS_ROLE_ARN` production C31 · pai `AWS_ROLE_ARN` aws-bootstrap C31 · OS C31 · Cobrança C31 · Execução C31 · Cadastros C31 · chave JWT C32 · `CHAVE_INTERNA` C32 · senha inicial C32 | - |
| parâmetros SSM novos (3) | `cluster_name` C27 · `oidc_provider_arn` C27 · `oidc_provider_url` C27 | - |
| deploy real, S6 (3) | com token 200 C33 · sem token 401 C33 · rollback C34 | - |

## Swept

- validation: C29 (serviço deduzido da role, chamada sem assinatura recusada), C16 (prefixos exatos)
- failure modes: C5 C6 C7 (migration que falha não troca a versão), C34 (`--atomic` restaura a revisão)
- idempotency: C29 (database existente), C32 (configuração do GitHub rodada duas vezes)
- authorization: C13 C24 (políticas avaliadas pelo simulador do IAM), C23 (trust por Environment), C29 (403), C2 C3 C4 (`/health` anônimo)
- concurrency: C8 C9 (só o Job do hook migra; réplicas não disputam a migration); dois deploys ao mesmo tempo: existing - lock DynamoDB do state do Terraform e o Helm recusa upgrade com release em `pending-*`
- data lifecycle: C29 (não recria a database nem troca a senha); `autoreparos_db` do Cadastros não muda: existing - D-34, o Terraform do Cadastros não declara o RDS
- dependency failure: C2 C3 C4 (`503` com a database fora), C33 (chave do Cadastros alcançada pelo ALB)
- state transitions: C34 (`failed` → anterior `deployed`)
- observability: C19 (o job falha visivelmente quando `/health` não responde); logs do Job de migration preservados por `hook-delete-policy` C14

## Handoff

- **Settled mid-build:** none
- **Dispatched:** build S1 (C1–C9) a um subagente em 2026-11-15, base `4606034` no pai e `feature/deploy-servicos` em OS `a6663d1`, Cobrança `92156d6`, Execução `d83badf` — returned (C1–C9 verdes)
- **Dispatched:** 2026-11-16, base no pai = commit das provas compartilhadas (`tf_test.sh`, `simular_iam.py`, módulos `_deploy_*`/`_iam_*`, [`contratos.md`](./contratos.md)); três subagentes de build em paralelo: (A) serviços S2–S4 + C35 (C10–C21, C35) em OS `036c92f`, Cobrança `58d7b87`, Execução `4de8a07`, Cadastros `528bcef`; (B1) plataforma C22–C27, C30–C32 e a parte de role de deploy do C17 em Infra.K8s `c13cfc5` e no pai; (B2) provisionador C28–C29 em Infra.Database `4d8c103` — pending
- **Boundary:** C1–C9 fechados em OS `036c92f`, Cobrança `58d7b87`, Execução `4de8a07`, pai `ac1add8` (provas reexecutadas pelo autor em 15/11)
- **Abandoned:** none
