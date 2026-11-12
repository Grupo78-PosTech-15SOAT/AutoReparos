# Cadastros checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.slnx`; `dotnet format submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.slnx --verify-no-changes`
Plan: `.specs/features/fase4/cadastros/plan.md`

9 checks · 2 slices · 2 one-way doors · 0 open questions, 0 blocking

## Checks

### S1 - Propriedade e contratos

**C1** - O App implantado como Cadastros não expõe rotas nem tabelas de OS, orçamento, catálogo, insumos ou dashboard (AC 1)
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedName~CadastrosNaoExpoeRotasNemTabelasMigradas`

**C2** - `GET /api/v1/clientes/{id}` e `GET /api/v1/veiculos/{id}` (e os mesmos em `/api/...`, D-28) respondem aos usuários autenticados e, sem usuário, só com a chave interna; o OS repassa o JWT do usuário e cliente ou veículo inexistente impede a abertura (AC 2, AC 7)
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedName~ConsultaClienteVeiculoPorJwtOuChaveInterna`
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~AberturaValidaClienteEVeiculoNoCadastros`

**C3** - Um serviço valida o JWT emitido pelo Cadastros pelo JWKS, sem segredo compartilhado, e o token leva o `kid` da chave ativa (AC 3, AC 8)
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedName~JwtValidadoPeloJwksComKid`

**C4** - `GET /.well-known/openid-configuration` e `/.well-known/jwks.json` são públicos e devolvem só a chave pública (AC 3)
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedName~DiscoveryEJwksPublicos`

**C5** - `POST /internal/clientes/autenticar` recebe CPF e e-mail, devolve o cliente e o JWT RS256 com `kid` (D-30) e autentica sem acesso da AuthLambda ao banco, e o SG do RDS deixa de liberar a Lambda (AC 4)
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedName~AutenticarClienteEndpointInterno`
Proof: `dotnet test submodules/AutoReparos.AuthLambda/tests --filter FullyQualifiedName~ClienteAuthFunctionTests`
Proof: `dotnet test submodules/AutoReparos.AuthLambda/tests --filter FullyQualifiedName~CadastrosClientTests`
Proof: `git grep -q -e "eks\]" -- .github/workflows/provision-aws.yml`
Proof: `git grep -q -e "eks\]" -- .github/workflows/destroy-aws.yml`
Proof: `grep -q ">= 1" submodules/AutoReparos.Infra.Database/terraform/variables.tf`

**C6** - O endpoint interno recusa chamada de fora do cluster ou sem `X-Chave-Interna` válido (AC 6)
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedName~EndpointInternoRecusaSemChave`
Proof: `helm template autoreparos submodules/AutoReparos.Infra.K8s/k8s --show-only charts/api/templates/networkpolicy.yaml`

### S2 - Notificações e CRUD mantido

**C7** - Evento de orçamento, conclusão, pagamento ou lembrete envia a notificação uma única vez por `eventId` (AC 5)
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedName~NotificacaoUmaVezPorEventId`

**C8** - Falha no envio do e-mail repete com backoff exponencial, envia à DLQ ao esgotar e só registra o `eventId` após o envio (AC 9)
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedName~FalhaDeEmailRepeteEVaiParaDlq`

**C9** - O CRUD de clientes, veículos e usuários continua com os contratos atuais do App, em `/api/v1/...` e `/api/...` (Surface, D-28)
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedName~CrudMantidoSemMudanca`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| `GET /api/v1/clientes/{id}` statuses (4) | 200 C2 · 401 C2 · 403 C2 · 404 C2 | - |
| `GET /api/v1/veiculos/{id}` statuses (4) | 200 C2 · 401 C2 · 403 C2 · 404 C2 | - |
| discovery and JWKS statuses (2) | 200 C4 · 404 C4 | - |
| `POST /internal/clientes/autenticar` statuses (4) | 200 C5 · 401 C6 · 403 C5 · 404 C5 | - |
| CRUD de clientes, veículos e usuários statuses (7) | 200 C9 · 201 C9 · 204 C9 · 400 C9 · 401 C9 · 403 C9 · 404 C9 | - |
| removed areas (5) | OS C1 · orçamento C1 · catálogo C1 · insumos C1 · dashboard C1 | - |
| notification events (4) | orçamento C7 · conclusão C7 · pagamento C7 · lembrete C7 | - |
| door 1: repositório `AutoReparos.Cadastros` com `cadastros_db` (1) | C1 | - |
| door 2: JWT RS256 com JWKS público (1) | C3 · C4 | - |

## Swept

- validation: C9
- failure modes: C8
- idempotency: C7
- authorization: C2, C6, C9
- concurrency: n/a - sem regra de concorrência nova; o CRUD mantido já tem seus guardas
- data lifecycle: C1
- dependency failure: C5, C8
- state transitions: n/a - Cadastros não tem máquina de estados nova
- observability: n/a - nenhum requisito de log ou métrica nesta fatia

## Handoff

- **Settled mid-build:** 2026-11-12 - D-31: 422 trocado por 400 em Surface e Coverage (claim de C9 inalterada); 2026-11-12 - caminhos do Build e das provas atualizados para os nomes pós-AD-025 (`AutoReparos.Cadastros.slnx`, `AutoReparos.Cadastros.IntegrationTests`); nenhuma claim mudou
- **Dispatched:** none
- **Boundary:** 2026-11-12 - lado Cadastros pronto em `submodules/AutoReparos.Cadastros`, branch local `feature/cadastros-reducao` (3 commits, sem push; 186 testes e `dotnet format` verdes): C1, C3, C4, C5 (endpoint), C6 (chave interna), C7, C8, C9 e a parte Cadastros de C2. OS pronto (C2): branch local `feature/os-valida-cadastros` em `submodules/AutoReparos.OS` (1 commit, sem push; 9 testes da prova passam, suíte do OS verde exceto `ServiceDatabaseIsolationTests`, que depende de `OS_PROOF_DB_CONNECTION` e já falhava antes). AuthLambda pronta (C5): branch local `feature/auth-lambda-cadastros` em `submodules/AutoReparos.AuthLambda` (1 commit, sem push; 49 testes verdes), sem driver de banco nem assinatura de token; o Cadastros passou a responder 403 a cliente inativo para a Lambda manter o contrato (Surface aditiva, commit `70b2479` na branch do Cadastros). Infra pronta: `submodules/AutoReparos.Infra.Database` branch `feature/rds-sem-lambda` (RDS só libera o SG do EKS, `terraform validate` ok), `submodules/AutoReparos.Infra.K8s` branch `feature/cadastros-rede-e-segredos` (NetworkPolicy, chave RS256, chave interna e variáveis de notificação no chart; `helm lint` ok) e, no pai, workflows, `.env.example`, compose, scripts e README (SG do RDS sem a Lambda, segredos novos, Lambda sem banco). Nada foi aplicado na AWS. Pendências: criar no GitHub os secrets `JWT_CHAVE_PRIVADA_PEM` e `CHAVE_INTERNA` e as variables `CADASTROS_ISSUER`, `CADASTROS_INTERNAL_URL`, `CADASTROS_NOTIFICACOES_FILA_URL`, `CADASTROS_NOTIFICACOES_DLQ_URL` e `VPC_PRIVATE_CIDRS`; o NetworkPolicy exige CNI com suporte (VPC CNI com NetworkPolicy habilitado); o `/internal` ainda passa pelo ingress nginx (o API Gateway só expõe `/api`, `/health` e web); renomear `db_name` do RDS para `cadastros_db` recria a instância e não foi feito; Verifier e `proofs.md`. Decidido (D-31): o CRUD responde 400, não 422. A decidir: os produtores precisam incluir `clienteId` nos eventos de notificação; o OS valida o `aud` do JWT (`Authentication:Audience`) e o Cadastros ainda não emite `aud`, então a configuração dos dois precisa ser alinhada na infra.
- **Abandoned:** none
- **Walked:** none - backend, sem tela
