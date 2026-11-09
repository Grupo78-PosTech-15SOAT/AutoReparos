# Fase 4 - Mapa de features

Esta pasta é a iniciativa e mantém os requisitos globais e decisões aprovadas. A implementação e os checks detalhados serão conduzidos em features TLC independentes, sem replicar os critérios completos neste mapa.

## Features dedicadas

| Feature | Escopo | Critérios de origem | Dependências principais |
| --- | --- | --- | --- |
| `os-service` | Ciclo de vida, status e histórico da OS; orquestração persistida da Saga; recepção da finalização e atualização de estado | AC 3 (OS), AC 6-7; decisões de pagamento após conclusão, ausência de expiração automática e intervenção operacional | `billing`, `execucao`, `saga-mensageria` |
| `billing` | Orçamento, aprovação, Checkout Pro, webhooks, cobrança após conclusão, tentativas, reconciliação e reembolsos auditáveis | AC 3 (orçamento/pagamento), AC 8; decisões de pagamento, retry manual, recuperação para frente e reembolso | `os-service` |
| `execucao` | Fila de execução, diagnóstico, reparos, conclusão, falha operacional e reexecução manual | AC 3 (execução); decisão de preservar etapas e exigir intervenção em falhas | `os-service` |
| `saga-mensageria` | Contratos REST/eventos SQS, estado da Saga em DynamoDB, compensações e consistência distribuída | AC 2, 5-7; decisões OS Service + DynamoDB + SQS e isolamento de dados | `os-service`, `billing`, `execucao` |
| `infra-ci-cd` | Repositórios independentes, IaC, bancos isolados, ambientes AWS/EKS, pipelines, cobertura, qualidade e proteção de branches | AC 1, 4, 9, 11-13; decisões RDS PostgreSQL, AWS/EKS e divisão de IaC | Base para todas as features de serviço |
| `observabilidade-entregaveis` | Telemetria distribuída, pacote dos repositórios, vídeo e PDF final | AC 14-17; decisões de reutilização da observabilidade da Fase 3 | Serviços e `infra-ci-cd` |

## Regras de decomposição

- Cada diretório acima receberá seu próprio `plan.md` com contratos, exemplos de falha e critérios EARS específicos; os checks serão derivados e validados contra esse plano.
- Os critérios AC originais são rastreabilidade da iniciativa. Uma feature pode detalhá-los, mas não alterar seu resultado exigido sem decisão registrada.
- Requisitos compartilhados ficam com um único dono: Saga/transportes em `saga-mensageria`; provisionamento e gates de pipeline em `infra-ci-cd`; telemetria e pacote final em `observabilidade-entregaveis`.
- As decisões já confirmadas em `fase4/plan.md` valem como restrições herdadas. Questões abertas permanecem abertas até a feature correspondente precisar delas.
- Nenhuma implementação é iniciada por este mapa; cada feature segue o gate TLC do próprio plano.
