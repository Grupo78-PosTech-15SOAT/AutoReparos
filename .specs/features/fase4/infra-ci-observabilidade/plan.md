# Fase 4 - Infraestrutura, CI/CD e observabilidade

**Revisão pendente de aprovação** (10/11/2026): inclui Cadastros e Web, SNS/SQS e DynamoDB por IaC, proteção com status checks obrigatórios e evidências pedidas no feedback da Fase 3 (D-13, D-19, D-22 de `.specs/features/fase4/especificacao/index.md`). A versão anterior foi aprovada em 2026-11-06 - user: "Pode seguir". Os `checks.md` desta feature precisam ser derivados de novo depois da aprovação.

## Problem

Os microsserviços da Fase 4 precisam ser implantáveis e verificáveis de forma independente, mas dependem de recursos de nuvem, gates de qualidade e observabilidade para operar em conjunto. Quando esta entrega estiver pronta, cada repositório terá pipeline verificável em Kubernetes e o fluxo distribuído será rastreável nos ambientes aprovados.

## Shape

Esta entrega reutiliza cluster EKS e observabilidade da Fase 3; IaC de recursos exclusivos fica nos repositórios de serviço e Infra.K8s mantém recursos compartilhados.

1. Push/PR nos repositórios OS, Cobranca, Execucao, Cadastros e Web aciona CI (door 1).
2. CI executa build, testes, gate de 80% de cobertura e SonarCloud com quality gate; os jobs são status checks obrigatórios nas branches protegidas (exists).
3. Pipeline autorizada (OIDC por repositório) publica imagem no ECR do serviço e implanta com Helm `--atomic` em namespace de staging; release aprovada promove a produção EKS (door 2). O pai mantém só o provisionamento da plataforma.
4. Cada serviço provisiona por Terraform próprio seu ECR, role IRSA, filas, assinaturas e DLQs, tópico SNS (se produtor) e, no OS, a tabela DynamoDB da Saga.
5. Serviços exportam logs, métricas e traces correlacionados para a stack da Fase 3 (contexto de trace propagado nas mensagens).

```mermaid
flowchart LR
    R[Repos dos serviços] -->|1 push/PR| CI[CI: build, testes, cobertura, qualidade door 1]
    CI --> IMG[Imagem Docker door 1]
    IMG --> ST[Staging EKS door 2]
    ST -->|release aprovada| PRD[Produção EKS door 2]
    ST --> OBS[Observabilidade Fase 3 exists]
    PRD --> OBS
```

Data: none - IaC e pipeline não alteram modelo de negócio; recursos são descritos em infraestrutura.

**Changes underneath**

| Front | What changes |
| --- | --- |
| domínio | Nenhuma semântica de domínio muda. |
| stored data | Nenhum dado de negócio migrado; IaC provisiona recursos e permissões. |
| readers | Pipelines e stack de observabilidade passam a ler métricas/logs/traces dos novos serviços. |

## Surface

None - nothing consumed outside as a public route; pipelines e recursos EKS/AWS são superfícies operacionais internas.

## Landing

| Door | Literal shape | Alternative rejected and why |
| --- | --- | --- |
| 1 | Build, testes, cobertura mínima 80% e qualidade como gates em cada repo | Pipeline única no pai não bloqueia regressões no ciclo independente do serviço. |
| 2 | Deploy em staging compartilhado EKS e produção somente após aprovação de release | Deploy direto à produção sem gate não é compatível com alvos aprovados. |

## Criteria

### Delivery and operations (P1)

**Acceptance Criteria**

1. WHEN PR elegível for aberto em cada serviço THEN CI SHALL executar build, testes automatizados, cobertura mínima 80% e análise de qualidade.
2. WHEN alterações forem integradas pela branch protegida `main` ou `develop` THEN repositório SHALL exigir PR e os jobs de build, testes e qualidade como status checks obrigatórios.
3. WHEN pipeline de staging passar THEN serviço SHALL ser implantado no namespace staging do EKS e expor resultado da implantação.
4. IF produção for alvo THEN deploy SHALL exigir aprovação de release e usar EKS na região `us-east-1`.
5. WHEN cada serviço processar uma OS THEN stack SHALL exibir logs, métricas e traces correlacionados entre serviços, atravessando SQS, SNS e DynamoDB Streams, e um dashboard da Saga com Sagas paradas, compensações e profundidade das DLQs.
6. IF AWS, banco ou mensageria tiver falha transitória THEN pipeline/serviço SHALL aplicar limites padrão, backoff exponencial com jitter e DLQ/alerta ao esgotar onde houver mensagem.
7. WHEN um serviço for implantado THEN seus recursos exclusivos (ECR, IRSA, filas, tópicos, DLQs, tabela DynamoDB, database e usuário) SHALL vir de IaC do próprio repositório, e a configuração de proteção de branches SHALL ter evidência registrada no README.

## Out of scope

| Excluded | Why |
| --- | --- |
| Criar conta AWS ou alterar credenciais | Reutiliza conta/role existente; segredos não entram nos arquivos. |
| Alterar código de negócio para instrumentação específica | Cada plano de serviço define instrumentação local necessária. |

## Assumptions

| Assumption | Chosen default | Rationale | Confirmed? |
| --- | --- | --- | --- |
| Alvos | Desenvolvimento local, staging namespace compartilhado, produção EKS após release, `us-east-1` | Confirmado no plano pai. | y - user: "Sim, adotar esses alvos" |
| Infra compartilhada | Serviços mantêm IaC exclusiva; Infra.K8s mantém cluster e observabilidade | Confirmado no plano pai. | y - user: "Adotar essa divisão" |
| Deploy | Cada repositório implanta o próprio serviço; o pai provisiona só a plataforma (D-13) | Mesmo sentido da door 1; registrado em `.specs/features/fase4/especificacao/index.md`. | y - José: aceitou a convergência em 10/11 |
| Mensageria | SNS por produtor + SQS por consumidor (D-19) | AD-014. | y - José: "Recomendado" |

**Open questions:** none

## Differences from the source

None - este plano não altera elementos do PDF binding.

## Sources

- `.specs/features/fase4/plan.md` - alvos, cobertura, CI e observabilidade confirmados.
- `.specs/features/fase4/checks.md` - AC 9, 11-15.
- `submodules/AutoReparos.Infra.K8s` - infraestrutura compartilhada existente.
- `.specs/features/fase4/especificacao/cicd/fase4_spec_cicd_e_protecao_de_branches.md`, `.specs/features/fase4/especificacao/infraestrutura/` e `.specs/features/fase4/especificacao/testes-qualidade/` - desenho detalhado.
