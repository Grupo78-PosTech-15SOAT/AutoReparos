# Deploy próprio do Web checks

Mutations: 3
Build: `helm lint submodules/AutoReparos.Web/deploy/helm/web`; `helm lint submodules/AutoReparos.Infra.K8s/k8s -f submodules/AutoReparos.Infra.K8s/k8s/values.yaml -f submodules/AutoReparos.Infra.K8s/k8s/values-production.yaml`; `terraform -chdir=submodules/AutoReparos.Infra.K8s/terraform/stacks/base fmt -check -recursive`
Plan: `.specs/features/fase4/web-deploy/plan.md`

17 checks in 5 slices · 2 one-way doors · 0 open, of which 0 block

Derivados em 2026-11-17, depois da aprovação ("Aprovo o plano do web-deploy com a D-72"). As regras de IaC, chart e workflow são provadas sem AWS (`terraform test` com `mock_provider`, `helm template`, leitura dos workflows); a política da role é avaliada pelo simulador do IAM (`aws iam simulate-custom-policy`, perfil `autoreparos`, sem criar recursos). Os checks marcados "no ar" leem a conta `198903849859`, o cluster `AutoReparosEKS` e o GitHub, e só passam depois da ordem (a)–(d) do AC 12.

## Checks

### S1 - Acesso de deploy na plataforma

**C1** - O `stacks/base` declara o namespace `web` com o rótulo `app.kubernetes.io/part-of: autoreparos`, a role `autoreparos-deploy-web` com trust exatamente em `repo:Grupo78-PosTech-15SOAT/AutoReparos.Web:environment:production` e `repo:Grupo78-PosTech-15SOAT@272379485/AutoReparos.Web@1454153026:environment:production` (aud `sts.amazonaws.com`), a política `autoreparos-deploy-web` renderizada de `deploy-web.json.tftpl` e a access entry com `AmazonEKSEditPolicy` e escopo `namespace` = `["web"]`; as 4 roles de serviço continuam iguais (AC 1)
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base web-deploy`
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base roles-deploy`

**C2** - No simulador do IAM, a política `deploy-web.json.tftpl` permite `ecr:GetAuthorizationToken` em `*`, `ecr:PutImage` e `ecr:BatchGetImage` em `repository/autoreparos-web` e `eks:DescribeCluster` em `cluster/AutoReparosEKS`, e nega `ecr:PutImage` em `repository/autoreparos-os`, `eks:DescribeCluster` em outro cluster, `sqs:SendMessage`, `sns:Publish`, `dynamodb:PutItem`, `secretsmanager:GetSecretValue`, `iam:CreateRole`, `iam:PassRole`, `ssm:GetParameter`, `s3:GetObject` no bucket de state e `lambda:InvokeFunctionUrl` no provisionador (AC 2)
Proof: `python3 scripts/specs/simular_iam.py --caso web`

**C3** - No ar: a role `autoreparos-deploy-web` existe na conta com as duas formas do `sub` do C1, uma única política inline igual à renderizada de `deploy-web.json.tftpl`, a access entry no `AutoReparosEKS` com `AmazonEKSEditPolicy` só no namespace `web`, e o namespace `web` existe no cluster com o rótulo `app.kubernetes.io/part-of=autoreparos` (AC 1, AC 12a)
Proof: `python3 scripts/specs/assert_web_deploy.py --check plataforma-no-ar`

### S2 - Chart próprio do Web

**C4** - `helm template web submodules/AutoReparos.Web/deploy/helm/web --namespace web` renderiza exatamente um `Deployment` e um `Service` `autoreparos-web` (porta `80` → `targetPort` `8080`, `ClusterIP`) e um Ingress `autoreparos-web-ingress` com `group.name: autoreparos-internal`, `group.order: "999"`, `scheme: internal`, `target-type: ip`, `healthcheck-path: /`, `ingressClassName: alb` e uma única regra `/` `Prefix` para o Service `autoreparos-web` porta `80`; `helm lint` sai com `0` (AC 3)
Proof: `python3 scripts/specs/assert_web_deploy.py --check chart`

**C5** - O container `web` do Deployment tem `OTEL_COLLECTOR_HOST=autoreparos-otel-collector.autoreparos.svc.cluster.local`, `API_URL` vazio, `imagePullPolicy: Always`, porta `8080`, probes em `/health`, requests `50m`/`64Mi` e limits `250m`/`128Mi`; com `--set-string image.repository=R --set-string image.tag=T` a imagem é `R:T` (AC 4)
Proof: `python3 scripts/specs/assert_web_deploy.py --check chart-config`

### S3 - Pipeline de deploy do Web

**C6** - O job `deploy` do `ci-cd.yml` do Web tem `needs: [build, test, quality]`, `environment: production`, `permissions` com `id-token: write`, assume `${{ vars.AWS_ROLE_ARN }}` com `aws-actions/configure-aws-credentials` (aud `sts.amazonaws.com`), monta a imagem com `docker build` pelo `Dockerfile` da raiz com tag `${{ github.sha }}`, publica no repositório `autoreparos-web`, conecta em `vars.EKS_CLUSTER_NAME` e roda, nessa ordem, `helm upgrade --install web deploy/helm/web --namespace web --atomic --wait --timeout 10m` com a imagem do ECR e `kubectl rollout status deployment/autoreparos-web -n web` (AC 5)
Proof: `python3 scripts/specs/assert_web_deploy.py --check deploy-job`

**C7** - Depois do rollout, o job faz `kubectl port-forward` para `service/autoreparos-web` e `GET /` em até `15` tentativas, e encerra com código diferente de `0` quando a última resposta não é `200` (AC 6)
Proof: `python3 scripts/specs/assert_web_deploy.py --check deploy-get`

**C8** - A condição do `deploy`, avaliada numa tabela de 10 casos, roda só em: push na `main` com build, test e quality `success`; `workflow_dispatch` na `main` com build e test `success` e (quality `success` ou `ignorar_quality: true`). É pulada com quality `failure` sem o input, com build ou test `failure`/`skipped` mesmo com o input, em `workflow_dispatch` na `develop` com o input, em push na `develop` e em `pull_request`. O input `ignorar_quality` é `boolean` com padrão `false` (AC 7)
Proof: `python3 scripts/specs/assert_web_deploy.py --check deploy-condicao`

**C9** - O `deploy` tem `concurrency.group: deploy-web-production` com `cancel-in-progress: false`; a concorrência do workflow não cancela na `main`; o `quality` roda em push e `workflow_dispatch` na `main` e em PR para a `main`, e não roda em push na `develop`; push na `develop` e PR disparam `build` e `test` (AC 8)
Proof: `python3 scripts/specs/assert_web_deploy.py --check deploy-concorrencia`

**C10** - No ar: o Environment `production` do `AutoReparos.Web` existe com as variáveis `AWS_ROLE_ARN=arn:aws:iam::198903849859:role/autoreparos-deploy-web`, `AWS_REGION=us-east-1` e `EKS_CLUSTER_NAME=AutoReparosEKS` (AC 12b)
Proof: `python3 scripts/specs/assert_web_deploy.py --check environment`

**C11** - No ar: o último run do `ci-cd.yml` na `main` do Web tem o job `deploy` com `success`, o log dele mostra `GET / respondeu 200`, e o Deployment `autoreparos-web` do namespace `web` roda a imagem `autoreparos-web:<headSha desse run>` com o rollout completo (AC 5, AC 6, AC 12c)
Proof: `python3 scripts/specs/assert_web_deploy.py --check deploy-no-ar`

### S4 - Quality verde

**C12** - No ar: o último run do `ci-cd.yml` em push na `main` do Web tem o job `quality` com `success` e o quality gate do SonarCloud da branch `main` está `OK` (AC 9)
Proof: `python3 scripts/specs/assert_web_deploy.py --check quality-verde`

### S5 - Pai deixa de implantar o Web

**C13** - No guarda-chuva do Infra.K8s, a dependência `web` do `k8s/Chart.yaml` tem `condition: web.enabled`; `values-production.yaml` tem `web.enabled: false`; `helm template` com `values.yaml` + `values-production.yaml` não renderiza nenhum objeto `autoreparos-web` nem o Ingress `autoreparos-web-ingress`, e com `--set web.enabled=true` renderiza o Deployment, o Service e o Ingress com `group.order: "1000"` (AC 10)
Proof: `python3 scripts/specs/assert_web_deploy.py --check guarda-chuva`

**C14** - O `deploy-app.yml` do pai não tem `docker build` nem `docker push` do Web, não referencia `submodules/AutoReparos.Web`, `ecr_web_repository_url`, `web.image` nem `deployment/autoreparos-web`, e continua com `helm upgrade --install autoreparos` com os dois arquivos de values (AC 10)
Proof: `python3 scripts/specs/assert_web_deploy.py --check pai-sem-web`

**C15** - O `destroy-aws.yml` roda `helm uninstall web --namespace web` e `kubectl delete ingress --all --namespace web` antes do laço que espera o fim dos load balancers da VPC (AC 11)
Proof: `python3 scripts/specs/assert_web_deploy.py --check destroy-web`

**C16** - No ar: não há Ingress `autoreparos-web-ingress` nem Deployment `autoreparos-web` no namespace `autoreparos`; o Ingress `autoreparos-web-ingress` do namespace `web` tem endereço no ALB do grupo `autoreparos-internal`; o release `autoreparos` tem `web.enabled=false` e a última revisão dele é posterior à primeira revisão `deployed` do release `web` (AC 10, AC 12d)
Proof: `python3 scripts/specs/assert_web_deploy.py --check rota-migrada`

**C17** - No ar: `GET /` pelo endpoint público do API Gateway (`/autoreparos/production/apigateway/endpoint`) responde `200` com HTML do Angular (`<app-root`) depois da migração (AC 12)
Proof: `python3 scripts/specs/assert_web_deploy.py --check raiz-publica`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| door 1 - dono do `/` (3 estados) | Ingress `web` ordem 999 renderizado C4 · guarda-chuva desligado não renderiza o Ingress 1000 C13 · no ar só o Ingress do `web` C16 | - |
| door 2 - role de deploy do Web: 2 formas do `sub` e 3 grupos de permissão (5) | `sub` clássico C1 · `sub` imutável C1 · `ecr:GetAuthorizationToken` C2 · `ecr:*` em `autoreparos-web` C2 · `eks:DescribeCluster` C2 · aplicado na conta C3 | - |
| ações negadas da role, AC 2 (10) | ECR de outro serviço C2 · outro cluster C2 · SQS C2 · SNS C2 · DynamoDB C2 · Secrets Manager C2 · `iam:CreateRole` C2 · `iam:PassRole` C2 · SSM C2 · S3 do state C2 · provisionador C2 (11 casos de negação) | - |
| condição do `deploy`, AC 7-8 (10 casos) | push `main` verde roda C8 · push `main` quality vermelho pula C8 · dispatch `main` com input e quality vermelho roda C8 · dispatch `main` sem input e quality vermelho pula C8 · dispatch `main` com input e test vermelho pula C8 · dispatch `main` com input e build vermelho pula C8 · dispatch `main` verde sem input roda C8 · dispatch `develop` com input pula C8 · push `develop` pula C8 · `pull_request` pula C8 | - |
| passos do job `deploy`, AC 5-6 (7) | OIDC C6 · build da imagem com `github.sha` C6 · push no ECR C6 · conectar no EKS C6 · `helm upgrade --install --atomic` C6 · rollout C6 · `GET /` 200 em 15 tentativas C7 | - |
| objetos do chart do Web (3) | Deployment C4 C5 · Service C4 · Ingress C4 | - |
| arquivos do pai que implantam ou destroem o Web (4) | `k8s/Chart.yaml` C13 · `values-production.yaml` C13 · `deploy-app.yml` C14 · `destroy-aws.yml` C15 | - |
| ordem da migração, AC 12 (4 passos) | (a) `stacks/base` C3 · (b) Environment C10 · (c) primeiro deploy C11 · (d) pai desligado depois do `web` C16 | - |
| variáveis do Environment `production` do Web (3) | `AWS_ROLE_ARN` C10 · `AWS_REGION` C10 · `EKS_CLUSTER_NAME` C10 | - |
| `Surface`: rotas HTTP novas (0) | nenhuma; o `/` continua servido, provado por C11 (Service) e C17 (público) | - |

## Swept

- validation: C8 (input `ignorar_quality` booleano com padrão `false`; a ref precisa ser `main`)
- failure modes: C6 (`--atomic` desfaz o upgrade que falha), C7 (`GET /` diferente de `200` falha o job)
- idempotency: C6 (`helm upgrade --install` serve para o primeiro deploy e os seguintes; a tag `github.sha` é a mesma num rerun)
- authorization: C1, C2, C3 (trust só no Environment `production` do Web; política mínima; edição só no namespace `web`); aprovação manual do Environment fica fora (Out of scope do plano)
- concurrency: C9 (`deploy-web-production` sem cancelar; o workflow não cancela na `main`)
- data lifecycle: n/a - o Web não guarda estado no servidor; a limpeza do release na destruição do ambiente é C15
- dependency failure: C7 (Service sem resposta falha o job depois de 15 tentativas); a falta da role ou do Environment falha o `configure-aws-credentials` antes do Helm (C6 põe o OIDC antes do Helm)
- state transitions: C13, C16 (`web.enabled` true → false só depois do primeiro deploy próprio)
- observability: C5 (`OTEL_COLLECTOR_HOST` pelo nome completo do collector do namespace `autoreparos`)

## Handoff

- **Settled mid-build:** none
- **Dispatched:** none
- **Boundary:** C1, C2, C4-C9 verdes local em 17/11 (Infra.K8s `b947a30` em `feature/web-deploy`; Web `687c3da`, `71e16a3`, `71d97d1` em `feature/web-deploy`; pai `1e34c89`, `1c93410`). C3, C10-C17 dependem da conta (apply do `stacks/base`, Environment, primeiro deploy, S5). Prova `deploy-job`: regex de `image.repository` corrigido para aceitar `${{ ... }}` (defeito do teste, claim igual)
- **Abandoned:** none
- **Walked:** n/a - nenhuma tela muda
