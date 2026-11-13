# Saga compensações checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx`; `dotnet format submodules/AutoReparos.OS/AutoReparos.OS.slnx --verify-no-changes`; `dotnet build submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx`; `dotnet format submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx --verify-no-changes`
Plan: `.specs/features/fase4/saga-compensacoes/plan.md`

14 checks · 3 slices · 2 one-way doors · 0 open questions, 0 blocking

Derivados de novo em 2026-11-12, depois da aprovação da revisão por D-32 e D-33.

## Checks

### S1 - Reserva e compensação

**C1** - `orcamento.aprovado.v1` com `itens` leva a Saga a `ReservandoInsumos` e publica `execucao.reservar-insumos.v1` com `ordemServicoId` e os mesmos `{insumoId, quantidade}` do evento, numa transação com a Inbox, antes de qualquer `ordem-servico.liberada.v1`; sem `itens` o evento não reserva e vai para a DLQ (AC 1)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~AprovacaoReservaAntesDeLiberar`
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~AprovacaoSemItensVaiParaDlq`

**C2** - `insumos.reservados.v1` leva a Saga a `ExecucaoLiberada` e publica `ordem-servico.liberada.v1` (AC 2)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~ReservadosLiberaExecucao`

**C3** - `insumos.reserva-falhou.v1` leva a Saga a `AguardandoInsumos` sem compensar, emite `saga.aguardando-insumos` uma vez por dia, e `retentar` reenvia a reserva enquanto o cancelamento compensa (AC 3)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~SemSaldoAguardaInsumos`

**C4** - Recusa do orçamento ou cancelamento antes da execução publica `execucao.cancelar-reserva.v1` (quando há reserva) e `cobranca.cancelar-orcamento.v1`, em ordem inversa, consome `insumos.reserva-cancelada.v1` como resultado do passo e termina em `Compensada` com o resultado de cada passo (AC 4)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CompensaReservaEOrcamentoEmOrdemInversa`

**C5** - Reserva ou liberação sem confirmação após o prazo e os reenvios compensa e termina em `Compensada` com motivo `liberacao_sem_resposta`, ou em `CompensacaoFalhou` com alerta se a compensação falhar (AC 5)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~PrazoSemRespostaCompensa`

**C6** - Cancelamento depois da conclusão da execução responde 409 e a reserva `Confirmada` permanece (AC 6)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CancelamentoAposConclusaoRecusado`

**C13** - A Cobrança publica `orcamento.aprovado.v1` com `itens` `{insumoId, quantidade}` do orçamento aprovado, mantendo `eventId` determinístico e os campos já existentes (AC 1, D-32)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~OrcamentoAprovadoCarregaItens`

**C13** - A Cobrança publica `orcamento.aprovado.v1` com `itens` `{insumoId, quantidade}` do orçamento aprovado, mantendo `eventId` determinístico e os campos já existentes (AC 1, D-32)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~OrcamentoAprovadoCarregaItens`

### S2 - Travas e concorrência

**C7** - Em `ReservandoInsumos` ou `Compensando`, `retentar` e novo cancelamento respondem 409 `saga_em_andamento` (AC 7)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~ComandosConflitantesRecebem409`

**C14** - Em `ReservandoInsumos` a Saga não publica `execucao.cancelar-reserva.v1` antes de `insumos.reservados.v1` ou `insumos.reserva-falhou.v1`; depois da confirmação o cancelamento publica o comando (AC 6a, D-33)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CancelarReservaSoDepoisDaConfirmacao`

**C14** - Em `ReservandoInsumos` a Saga não publica `execucao.cancelar-reserva.v1` antes de `insumos.reservados.v1` ou `insumos.reserva-falhou.v1`; depois da confirmação o cancelamento publica o comando (AC 6a, D-33)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CancelarReservaSoDepoisDaConfirmacao`

**C8** - Dois eventos concorrentes para a mesma Saga, inclusive nos estados novos, gravam uma única transição por escrita condicional de versão (AC 8)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~EventosConcorrentesGravamUmaTransicao`

### S3 - Prazos, lembretes e operação

**C9** - Prazo técnico vencido (`PrazoReserva` 2 min, `PrazoLiberacao` 2 min, `PrazoCompensacao` 15 min, até 3 reenvios) reenvia o passo e, esgotados os reenvios, compensa ou marca intervenção com alerta (AC 9)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~PrazoTecnicoReenviaEEsgota`

**C10** - Prazo de negócio da Saga vencido (`AguardandoPagamento` ou `AguardandoInsumos`) publica `saga.lembrete.v1` e nunca expira nem cancela o orçamento (AC 10)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~LembreteNaoExpiraOrcamento`

**C11** - Consultar, listar e retentar Sagas executa a ação e registra autor, data e motivo no histórico, aplicado à OS por mensagem de transição, sem gravar nos dois bancos (AC 11)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~ConsoleAdminRegistraAcao`

**C12** - `POST /api/v1/admin/dlq/reprocessar` devolve à fila de origem até a quantidade pedida e registra a ação, em OS, Cobrança e Execução (AC 12)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~ReprocessaDlqDoOs`
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~ReprocessaDlqDaCobranca`
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~ReprocessaDlqDaExecucao`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| `POST /api/v1/ordens-servico/{id}/cancelamento` statuses (6) | 202 C4 · 401 C6 · 403 C6 · 404 C6 · 409 C7 · 422 C6 | - |
| `orcamento.aprovado.v1` campo `itens` (2) | com itens C1 · C13 · sem itens C1 | - |
| ordem do cancelamento em `ReservandoInsumos` (2) | antes da confirmação C14 · depois da confirmação C14 | - |
| `GET /api/v1/admin/sagas` statuses (3) | 200 C11 · 401 C11 · 403 C11 | - |
| `GET /api/v1/admin/sagas/{ordemServicoId}` statuses (4) | 200 C11 · 401 C11 · 403 C11 · 404 C11 | - |
| `POST /api/v1/admin/sagas/{ordemServicoId}/retentar` statuses (5) | 202 C3 · 401 C11 · 403 C11 · 404 C11 · 409 C7 | - |
| `POST /api/v1/admin/dlq/reprocessar` statuses (4) | 202 C12 · 401 C12 · 403 C12 · 422 C12 | - |
| DLQ reprocessing services (3) | OS C12 · Cobrança C12 · Execução C12 | - |
| new Saga states (2) | `ReservandoInsumos` C1 · `AguardandoInsumos` C3 | - |
| compensation end states (3) | `Compensada` C4 · `Compensada` com `liberacao_sem_resposta` C5 · `CompensacaoFalhou` C5 | - |
| door 1: estados novos no DynamoDB `autoreparos-os-saga` (1) | C1 · C8 | - |
| door 2: comandos por SQS direto com envelope herdado (1) | C1 · C4 | - |
| serviços alterados (2) | OS C1-C12 · Cobrança C13 | - |

## Swept

- validation: C1, C11, C12
- failure modes: C5, C9
- idempotency: C8
- authorization: C11, C12
- concurrency: C7, C8, C14
- data lifecycle: C11
- dependency failure: C5, C9
- state transitions: C1-C6, C14
- observability: C3, C9

## Handoff

- **Settled mid-build:** 2026-11-12 - "Worktree novo da saga" (build em `.claude/worktrees/saga-compensacoes`, branch `feature/saga-compensacoes`); orçamento sem insumos (`itens` vazio) vai direto a `ExecucaoLiberada` e `itens` ausente vai à DLQ; etapa 5 "Insumos reservados" só existe quando há insumos
- **Dispatched:** none
- **Boundary:** fechados C1, C2, C3, C4, C6, C7, C8, C11 (parcial: lista, detalhe e retentar) e C13, C14 (Cobrança `6e54a04`; OS `6c327a3`, `c22788f`, `38374a2`; testes `SagaInsumosTests`, `SagaConsoleRotaTests`, `SagaConcorrenciaTests`). Faltam C5, C9, C10 e C12 (prazos, lembrete e reprocessamento de DLQ nos três serviços); `check_proofs.py` e o Verifier não rodaram. Desvio a confirmar: testes do OS em `AutoReparos.OS.Tests`, não em `Api.Tests`/`IntegrationTests`
- **Abandoned:** none
- **Walked:** none - backend, sem tela
