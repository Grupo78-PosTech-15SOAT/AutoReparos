# Fase 4 - Mapa de features

Esta pasta é a iniciativa e mantém os requisitos globais e decisões aprovadas. O registro único de decisões e o desenho transversal (Saga, contratos, bancos) ficam na subpasta [`especificacao/`](especificacao/index.md), que não é uma feature TLC (§ 3.1 do índice explica a divisão de papéis e o § 8 o protocolo de preenchimento). A implementação e os checks detalhados serão conduzidos em features TLC independentes, sem replicar os critérios completos neste mapa.

## Features dedicadas

| Feature | Escopo | Critérios de origem | Dependências principais |
| --- | --- | --- | --- |
| `servico-os` | Ciclo de vida, status e histórico da OS; orquestração persistida da Saga; recepção da finalização e atualização de estado | AC 3 (OS), AC 6-7; decisões de pagamento após conclusão, ausência de expiração automática e intervenção operacional | `cobranca`, `execucao`, `saga-mensageria` |
| `cobranca` | Orçamento, aprovação, Checkout Pro, webhooks, cobrança após conclusão, tentativas, reconciliação e reembolsos auditáveis | AC 3 (orçamento/pagamento), AC 8; decisões de pagamento, retry manual, recuperação para frente e reembolso | `servico-os` |
| `execucao` | Fila de execução, diagnóstico, reparos, conclusão, falha operacional e reexecução manual | AC 3 (execução); decisão de preservar etapas e exigir intervenção em falhas | `servico-os` |
| `saga-mensageria` | Contratos REST/eventos SQS, estado da Saga em DynamoDB, compensações e consistência distribuída | AC 2, 5-7; decisões OS Service + DynamoDB + SQS e isolamento de dados | `servico-os`, `cobranca`, `execucao` |
| `saga-compensacoes` | Evolução da Saga (D-18, AD-021): reserva de insumos em duas fases, estados `AguardandoInsumos`/`FalhaLiberacao`/`FalhaExecucao`/`Cancelada`, travas 409, prazos e lembretes, console admin e reprocessamento de DLQ | AC 2, 5-7; D-18, D-19, D-20, L-01 | `saga-mensageria`, `insumos` |
| `eventos-portugues` | Padronização dos tipos de mensagem já implementados; comando de cancelamento da Cobrança em português com leitura de mensagens legadas | AC 5; D-24, L-02 | `saga-mensageria`; OS e Cobrança |
| `insumos` | Catálogo de serviços e insumos na Execução; reserva em duas fases (reservar, confirmar, cancelar) | AC 3 (execução), AC 7; AD-011, AD-013 | `execucao`, `saga-mensageria` |
| `cadastros` | App reduzido a Cadastros: clientes, veículos, usuários, emissão de JWT/JWKS, endpoint interno para a AuthLambda, notificações por eventos | AC 1-2; AD-013 | `servico-os` |
| `web` | Extrair o Angular existente do Cadastros para a `develop` de `AutoReparos.Web`, com build, CI e referências do pai; novas APIs e telas em etapa posterior | D-03, AD-028 | `cadastros`, `infra-ci-observabilidade` |
| `infra-ci-observabilidade` | Repositórios independentes, IaC, bancos isolados, ambientes AWS/EKS, pipelines, cobertura, qualidade e proteção de branches | AC 1, 4, 9, 11-13; decisões RDS PostgreSQL, AWS/EKS e divisão de IaC | Base para todas as features de serviço |
| `pacote-entrega` | Pacote dos repositórios, vídeo e PDF final (a telemetria distribuída ficou em `infra-ci-observabilidade`) | AC 14-17; decisões de reutilização da observabilidade da Fase 3 | Serviços e `infra-ci-observabilidade` |

Nomes de repositório em português (AD-010): `cobranca` corresponde a `AutoReparos.Cobranca` e `execucao` a `AutoReparos.Execucao`; em 11/11 as solutions, os projetos e os namespaces dos repositórios foram renomeados (`AutoReparos.Cobranca.*`, `AutoReparos.Execucao.*`, `AutoReparos.OS.*`, `AutoReparos.Cadastros.*`) e a pasta `billing` virou `cobranca` (AD-025). A pasta da feature `os-service` virou `servico-os` (D-27, AD-029).

## Regras de decomposição

- Cada diretório acima receberá seu próprio `plan.md` com contratos, exemplos de falha e critérios EARS específicos; os checks serão derivados e validados contra esse plano.
- Os critérios AC originais são rastreabilidade da iniciativa. Uma feature pode detalhá-los, mas não alterar seu resultado exigido sem decisão registrada.
- Requisitos compartilhados ficam com um único dono: Saga/transportes em `saga-mensageria`; provisionamento, gates de pipeline e telemetria em `infra-ci-observabilidade`; pacote final em `pacote-entrega`.
- As decisões já confirmadas em `fase4/plan.md` valem como restrições herdadas. Questões abertas permanecem abertas até a feature correspondente precisar delas.
- Nenhuma implementação é iniciada por este mapa; cada feature segue o gate TLC do próprio plano.
