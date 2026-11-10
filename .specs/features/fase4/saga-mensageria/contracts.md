# Contrato de mensagens da Saga (v1)

Documento de projeto da feature `saga-mensageria`. Fonte única para OS Service (orquestrador), Billing e Execution.
Decisões de projeto desta rodada (ver AD-010 no `STATE.md`); o `plan.md` aprovado não foi alterado.

## Princípios

- **Orquestração persistida no OS Service.** O estado da Saga fica em uma tabela DynamoDB própria do OS (`autoreparos-os-saga`). Billing e Execution não leem esse estado: só trocam mensagens SQS.
- **A Saga nasce da aprovação do orçamento.** O OS Service cria a Saga ao receber `orcamento.aprovado.v1`. Isso evita escrita dupla (Postgres e DynamoDB) na criação da OS. A OS existe antes; a Saga referencia `ordemServicoId`.
- **Persistir antes de publicar (AC 1).** Toda transição grava o novo estado e o `correlacaoId` no DynamoDB (escrita condicional por versão) antes de publicar o comando correspondente.
- **Entrega pelo menos uma vez, efeito uma vez (AC 3).** Todo consumidor deduplica por `eventId` com registro persistido; mensagem repetida não repete efeito.
- **Sem estorno automático (AC 5, AD-001).** Pagamento capturado nunca gera reembolso por falha da Saga: a Saga segue para frente, retenta com backoff e alerta.
- **Retry técnico (AC 6, AD-001).** 5 tentativas, backoff exponencial com jitter, depois DLQ e alerta acionável (mesma política de `DependencyRetry` do Billing e do Execution).

## Envelope comum (JSON, chaves em camelCase)

```json
{
  "eventId": "uuid",
  "tipo": "nome.da.mensagem.v1",
  "correlacaoId": "uuid da Saga (ausente só em orcamento.aprovado.v1 de primeira entrega)",
  "ordemServicoId": "uuid",
  "ocorreuEm": "2026-11-10T12:00:00Z"
}
```

Demais campos dependem do `tipo`. `eventId` é gerado por quem publica e é estável em reenvios.

## Estados da Saga (OS Service)

| Estado | Significado | Terminal |
| --- | --- | --- |
| `OrcamentoAprovado` | Saga criada; sincronizando a OS | não |
| `ExecucaoLiberada` | comando `ordem-servico.liberada.v1` publicado | não |
| `ExecucaoConcluida` | execução concluiu; aguardando pagamento | não |
| `IntervencaoRequerida` | execução falhou; aguarda reexecução manual | não |
| `PagamentoCapturado` | pagamento capturado e OS sincronizada | sim |
| `Compensando` | compensações em andamento (ordem inversa) | não |
| `Compensada` | todas as compensações confirmadas | sim |
| `CompensacaoFalhou` | alguma compensação foi recusada ou esgotou; exige intervenção e gera alerta | sim |

Estado da OS (Postgres) sincronizado a cada transição, com registro no histórico: `Recebida` → `OrcamentoAprovado` → `EmExecucao` → `ServicoConcluido` → `Paga`; desvios: `IntervencaoRequerida`, `Cancelada`. A sincronização é idempotente (mesmo estado não duplica histórico). Falha de sincronização usa recuperação para frente (retry com backoff, alerta, nova tentativa periódica), nunca desfaz pagamento.

## Etapas e compensações

| Etapa | Efeito | Compensação declarada |
| --- | --- | --- |
| 1. Orçamento aprovado | Billing aprovou o orçamento | `billing.orcamento.cancelar.v1` |
| 2. Execução liberada | Execution recebeu a OS | `execucao.cancelar.v1` |
| 3. Execução concluída | trabalho feito | nenhuma (só recuperação para frente) |
| 4. Pagamento capturado | dinheiro recebido | nenhuma (nunca estornar automaticamente) |

Falha de etapa compensável (`execucao.liberacao-recusada.v1`) ou cancelamento (`ordem-servico.cancelar.v1`) com etapas 1 ou 2 concluídas: compensar as etapas concluídas em ordem inversa, registrar resultado ou erro de cada compensação no estado da Saga. Etapas 3 e 4 concluídas tornam o cancelamento recusado e registrado (alerta), sem estorno.

## Mensagens

### Entrada do OS Service (fila `Sqs:SagaQueueUrl`, DLQ `Sqs:SagaDeadLetterQueueUrl`)

| `tipo` | Origem | Campos extras | Efeito |
| --- | --- | --- | --- |
| `orcamento.aprovado.v1` | Billing | `orcamentoId`, `clienteId` | cria a Saga (`ordemServicoId` único), vai a `OrcamentoAprovado`, sincroniza a OS, avança para `ExecucaoLiberada` publicando `ordem-servico.liberada.v1` |
| `execucao.concluida.v1` | Execution | `execucaoId` | `ExecucaoConcluida`; OS `ServicoConcluido` |
| `execucao.falhou.v1` | Execution | `execucaoId`, `motivo` | `IntervencaoRequerida`; OS `IntervencaoRequerida`; nenhuma compensação (decisão da fase 4) |
| `execucao.liberacao-recusada.v1` | Execution | `motivo` | falha da etapa 2: compensa a etapa 1 |
| `pagamento.resultado.v1` | Billing | `pagamentoId`, `status` (`Capturado`, `Rejeitado`, `Cancelado`, `Expirado`, `Reembolsado`), `referencia` | `Capturado`: `PagamentoCapturado` e OS `Paga`; demais: registra no histórico sem mudar a etapa |
| `ordem-servico.cancelar.v1` | interna | `motivo` | compensa etapas concluídas em ordem inversa; recusado se a etapa 3 ou 4 já ocorreu |
| `orcamento.cancelado.v1` | Billing | `orcamentoId` | confirma a compensação 1 |
| `orcamento.cancelamento-recusado.v1` | Billing | `orcamentoId`, `motivo` | `CompensacaoFalhou` e alerta |
| `execucao.cancelada.v1` | Execution | `execucaoId` | confirma a compensação 2 |
| `execucao.cancelamento-recusado.v1` | Execution | `execucaoId`, `motivo` | `CompensacaoFalhou` e alerta |

### Comandos publicados pelo OS Service

| `tipo` | Destino (config no OS) | Campos extras |
| --- | --- | --- |
| `ordem-servico.liberada.v1` | `Sqs:ExecucaoQueueUrl` (= `Sqs:LiberadasQueueUrl` do Execution) | `clienteId`, `veiculoId`, `id` (= `ordemServicoId`, compatível com o contrato atual do Execution) |
| `execucao.cancelar.v1` | `Sqs:ExecucaoQueueUrl` | `motivo` |
| `billing.orcamento.cancelar.v1` | `Sqs:BillingQueueUrl` (= `Sqs:ComandosQueueUrl` do Billing) | `orcamentoId`, `motivo` |

### Billing (mudanças)

- Publica `orcamento.aprovado.v1` na aprovação do orçamento (na mesma transação, via mecanismo de retry/DLQ já existente ou outbox) para a fila de entrada da Saga do OS (`Sqs:QueueUrl` existente).
- Publica `pagamento.resultado.v1` com o envelope completo (hoje publica só `pagamentoId`, `status`, `referencia`).
- Consome `billing.orcamento.cancelar.v1` (fila `Sqs:ComandosQueueUrl`, DLQ própria), deduplica por `eventId`: cancela o orçamento se `Pendente` ou `Aprovado` sem pagamento (novo estado `Cancelado`); se houver pagamento, recusa. Responde `orcamento.cancelado.v1` ou `orcamento.cancelamento-recusado.v1`.

### Execution (mudanças)

- A fila `Sqs:LiberadasQueueUrl` passa a aceitar, além de `ordem-servico.liberada.v1`, o comando `execucao.cancelar.v1`: cancela a entrada se `Fila` ou `EmExecucao` (novo estado `Cancelada`); recusa se `Concluida`. Responde `execucao.cancelada.v1` ou `execucao.cancelamento-recusado.v1` para a fila de entrada da Saga do OS (`Sqs:OsServiceQueueUrl`).
- `execucao.concluida.v1` e `execucao.falhou.v1` ganham `correlacaoId` e `ordemServicoId` quando conhecidos; mantêm o contrato atual (`execucaoId`, `motivo`).
- Quando uma mensagem `ordem-servico.liberada.v1` válida em estrutura é recusada por regra de negócio, publica `execucao.liberacao-recusada.v1` além do tratamento atual de DLQ para mensagens inválidas.

## README (AC 7)

O README do OS Service ganha a seção `## Saga` com: estratégia **orquestrada**, estado persistido em **DynamoDB**, justificativa contra coreografia (visibilidade, retomada, compensação ordenada, auditoria), diagrama das etapas e este contrato resumido. O check C7 roda `scripts/assert_saga_readme.py` no repositório do OS.

## Fora de escopo desta rodada

- Rota REST pública de cancelamento (o plano declara Surface vazia; o cancelamento entra por `ordem-servico.cancelar.v1`).
- Job de reconciliação de reservas de reembolso `Pendente` no Billing.
- Provisionamento real de DynamoDB e SQS na AWS (feature `infra-ci-observabilidade`); os testes usam DynamoDB Local e SQS falso.
