# Infraestrutura, CI/CD e observabilidade - checks

Mutations: 3
Build: none - a feature cria workflows, charts e Terraform que ainda não existem nos repositórios de serviço; `terraform validate` e `helm lint` de cada repositório são provados por C13
Plan: `.specs/features/fase4/infra-ci-observabilidade/plan.md`

14 checks em 4 slices · 2 one-way doors · 1 open, of which 0 block

## Checks

Repositórios de serviço, nesta ordem em todo o arquivo: `AutoReparos.OS`, `AutoReparos.Cobranca`, `AutoReparos.Execucao`, `AutoReparos.Cadastros`, `AutoReparos.Web` (org `Grupo78-PosTech-15SOAT`). Os scripts `scripts/specs/assert_*.py` leem os workflows e a IaC dos submódulos e a API do GitHub por `gh api`; saem com código diferente de zero e o nome do repositório e do item quando algo falta.

### S1 - Pipeline de CI por repositório (door 1)

**C1** - Cada um dos 5 repositórios tem `.github/workflows/ci-cd.yml` disparado por `pull_request` para `main` e por `push` em `develop` e `main`, com os jobs `build`, `test` e `quality` (AC 1)
Proof: `python scripts/specs/assert_pipelines.py --check jobs`

**C2** - O job `test` de cada repositório falha quando a cobertura de linhas fica abaixo de 80%: limiar `80` configurado no comando de cobertura do job, sem `continue-on-error` (AC 1)
Proof: `python scripts/specs/assert_pipelines.py --check cobertura --minimo 80`

**C3** - O job `quality` de cada repositório roda o SonarCloud do próprio projeto com `sonar.qualitygate.wait=true`, sem `continue-on-error` (AC 1)
Proof: `python scripts/specs/assert_pipelines.py --check qualidade`

**C4** - A última execução de `ci-cd.yml` disparada por PR em cada um dos 5 repositórios terminou com `build`, `test` e `quality` em `success` (AC 1)
Proof: `python scripts/specs/assert_pipelines.py --check execucao-pr`

### S2 - Branch `main` protegida

**C5** - A `main` de cada um dos 5 repositórios exige PR com pelo menos 1 aprovação, `dismiss_stale_reviews` ligado, status checks `strict` contendo `build`, `test` e `quality`, `enforce_admins` ligado, e bloqueia force push e exclusão (AC 2)
Proof: `python scripts/specs/assert_branch_protection.py --check main`

**C6** - Um PR com teste quebrado num repositório protegido aparece no GitHub como não mesclável (botão de merge bloqueado pelo check `test`) e é fechado sem merge (AC 2)
Proof: walk - abrir PR com teste falhando em `AutoReparos.Execucao`, ver o merge bloqueado, fechar o PR e apagar a branch

**C7** - O README de cada um dos 5 repositórios tem a seção `## Proteção de branch` com a saída de `gh api repos/<org>/<repo>/branches/main/protection` mostrando `build`, `test` e `quality` (AC 7)
Proof: `python scripts/specs/assert_branch_protection.py --check readme`

### S3 - Deploy em staging e produção (door 2)

**C8** - `ci-cd.yml` de cada repositório tem o job `deploy` que, em `push` na `develop`, assume a role por OIDC, roda `helm upgrade --install --atomic` do chart `deploy/helm/<servico>` no namespace `staging` e depois `kubectl rollout status` (AC 3)
Proof: `python scripts/specs/assert_pipelines.py --check deploy-staging`

**C9** - A última execução do job `deploy` em `develop` de cada repositório terminou em `success` e registrou o resultado do rollout no resumo do job (AC 3)
Proof: `python scripts/specs/assert_deployment.py --ambiente staging`

**C10** - O deploy de produção roda só em `push` na `main`, usa o Environment `production` com pelo menos 1 revisor obrigatório e a região `us-east-1` (AC 4)
Proof: `python scripts/specs/assert_pipelines.py --check deploy-producao`
Proof: `python scripts/specs/assert_deployment.py --ambiente producao`

### S4 - IaC, mensageria e observabilidade

**C11** - Toda fila SQS declarada no Terraform dos serviços tem DLQ por `redrive_policy` com `maxReceiveCount` entre 3 e 10 e um alarme CloudWatch de profundidade da DLQ maior que 0 (AC 6)
Proof: `python scripts/specs/assert_iac.py --check dlq`

**C12** - Cada repositório de serviço declara no próprio `infra/terraform` os recursos exclusivos do seu serviço: ECR e role IRSA em todos; filas, DLQs, database e usuário nos 4 de backend; tópico SNS em OS, Cobrança e Execução; tabela DynamoDB `autoreparos-os-saga` só no OS (AC 7)
Proof: `python scripts/specs/assert_iac.py --check recursos`

**C13** - `terraform validate` e `helm lint` passam em cada repositório de serviço (AC 7)
Proof: `python scripts/specs/assert_iac.py --check validar`

**C14** - Uma OS processada em staging gera um trace no Grafana/Tempo com spans de OS, Cobrança e Execução sob o mesmo `traceId` atravessando SQS e SNS, e o dashboard `Saga` mostra os painéis `Sagas paradas`, `Compensações` e `Profundidade das DLQs` (AC 5)
Proof: `python scripts/specs/assert_observabilidade.py --check dashboard-saga`
Proof: walk - criar e concluir uma OS em staging, abrir o trace pelo `traceId` do log do OS e conferir os três serviços e o dashboard `Saga`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| repositórios de serviço (5) | OS C1-C5,C7-C13 · Cobranca C1-C5,C7-C13 · Execucao C1-C5,C7-C13 · Cadastros C1-C5,C7-C13 · Web C1-C5,C7-C10,C12,C13 | - |
| jobs obrigatórios (3) | `build` C1,C4,C5 · `test` C1,C2,C4,C5,C6 · `quality` C1,C3,C4,C5 | - |
| regras da proteção da `main` (6) | PR com 1 aprovação C5 · `dismiss_stale_reviews` C5 · checks `strict` C5 · `enforce_admins` C5 · sem force push C5 · sem exclusão C5 | - |
| ambientes alterados (2) | staging C8,C9 · produção `us-east-1` com aprovação C10 | - |
| doors (2) | door 1 C1-C4 · door 2 C8-C10 | - |
| recursos exclusivos por serviço (7) | ECR C12 · role IRSA C12 · filas C12 · DLQs C11,C12 · tópico SNS C12 · tabela DynamoDB C12 · database e usuário C12 | - |
| painéis do dashboard da Saga (3) | `Sagas paradas` C14 · `Compensações` C14 · `Profundidade das DLQs` C14 | - |
| transportes do trace (2) | SQS C14 · SNS C14 | - |

## Swept

- validation: C2, C3, C13
- failure modes: C6 (merge bloqueado), C8 (`--atomic` desfaz release sem readiness)
- idempotency: C8 (`helm upgrade --install` reaplicável)
- authorization: C5, C10; OIDC por repositório em C8
- concurrency: n/a - o GitHub serializa merges com `strict`; deploys concorrentes ficam com o `concurrency` do workflow, sem requisito no plano
- data lifecycle: n/a - nenhum dado de negócio é criado ou migrado
- dependency failure: C11; retry com backoff e jitter nos serviços: existing - checks de `execucao` C6, `cobranca` e `saga-mensageria`
- state transitions: C10 (staging antes de produção, produção só após aprovação)
- observability: C14

## Handoff

- **Settled mid-build:** 2026-11-11 - "Branch protegida: main"; "Repositório Web: Vamos criar"; "Sim, aprovo e crio" (AD-026); "Pode seguir" - AC 5 sem DynamoDB Streams (AD-027)
- **Open (não bloqueia):** os repositórios de plataforma (AuthLambda, Infra.Database, Infra.K8s) e o pai já têm a `main` com PR e 1 aprovação, mas sem status checks; o plano cobre só os 5 serviços. Incluí-los com os jobs de validação deles é decisão do usuário
- **Dispatched:** none
- **Boundary:** checks derivados do plano aprovado em 2026-11-11; build não iniciado
- **Abandoned:** checks de 06/11 (C1-C6, 3 repositórios e nomes antigos) substituídos por estes
- **Walked:** none - C6 e C14 têm `walk` a fazer no build
