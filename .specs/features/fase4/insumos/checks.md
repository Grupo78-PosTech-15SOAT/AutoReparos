# Insumos checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.Execucao/AutoReparos.Execucao.slnx`; `dotnet format submodules/AutoReparos.Execucao/AutoReparos.Execucao.slnx --verify-no-changes`
Plan: `.specs/features/fase4/insumos/plan.md`

10 checks · 2 slices · 1 one-way door · 0 open questions, 0 blocking

## Checks

### S1 - Reserva em duas fases

**C1** - `execucao.reservar-insumos.v1` com `ordemServicoId` e itens reserva todos os itens numa transação com a Inbox e grava a Outbox `insumos.reservados.v1` (AC 1)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~ReservaTodosItensComOutbox`

**C2** - Item sem saldo: nenhuma reserva é gravada e `insumos.reserva-falhou.v1` lista os itens em falta (AC 2)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~FaltaDeSaldoNaoReservaNada`

**C3** - O mesmo comando, ou outro comando da mesma OS, não reserva o dobro (AC 3)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~ReservaNaoDuplica`

**C4** - Duas reservas concorrentes do mesmo insumo nunca deixam `QuantidadeDisponivel` negativa (AC 1, AC 2)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~ReservasConcorrentesRespeitamSaldo`

**C5** - A conclusão da execução confirma as reservas na mesma transação (AC 4)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~ConclusaoConfirmaReservas`

**C6** - `execucao.cancelar-reserva.v1` devolve o saldo das reservas `Reservada`, publica `insumos.reserva-cancelada.v1` e mantém reserva `Confirmada` (AC 5)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~CancelarReservaDevolveSaldo`

**C7** - Ordem de execução `Cancelada` recusa etapas e conclusão com 409 (AC 6)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~ExecucaoCanceladaRecusa409`

### S2 - Catálogo

**C8** - `GET /api/v1/servicos` e `GET /api/v1/insumos` devolvem `PrecoUnitario` e, nos insumos, saldos disponível e reservado (AC 7)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~CatalogoExpoePrecoESaldos`

**C9** - `POST /api/v1/insumos/{id}/entradas` aumenta o saldo disponível e registra a movimentação (AC 8)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~EntradaAumentaSaldoEMovimentacao`

**C10** - O orçamento da Cobrança grava o snapshot do `PrecoUnitario`; reajuste posterior não altera orçamento aprovado (AC 9)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~OrcamentoGuardaSnapshotDePreco`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| `GET /api/v1/servicos` statuses (2) | 200 C8 · 401 C8 | - |
| `POST /api/v1/servicos` statuses (4) | 201 C8 · 401 C8 · 403 C8 · 422 C8 | - |
| `GET /api/v1/insumos` statuses (2) | 200 C8 · 401 C8 | - |
| `POST /api/v1/insumos` statuses (4) | 201 C8 · 401 C8 · 403 C8 · 422 C8 | - |
| `POST /api/v1/insumos/{id}/entradas` statuses (5) | 200 C9 · 401 C9 · 403 C9 · 404 C9 · 422 C9 | - |
| reservation statuses (3) | `Reservada` C1 · `Confirmada` C5 · `Cancelada` C6 | - |
| reservation scenarios (4) | all items C1 · missing stock C2 · repeated command C3 · concurrency C4 | - |
| door 1: catalog and reservations in Execucao PostgreSQL (1) | `UPDATE ... WHERE disponivel >= q` in the Inbox/Outbox transaction C1 · C4 | - |

## Swept

- validation: C2, C9
- failure modes: C2, C5
- idempotency: C3
- authorization: C8, C9
- concurrency: C4
- data lifecycle: C10
- dependency failure: n/a - a reserva usa só o banco da Execução e o comando traz os itens
- state transitions: C5, C6, C7
- observability: n/a - nenhum requisito de log ou métrica nesta fatia

## Handoff

- **Settled mid-build:** none
- **Dispatched:** none
- **Boundary:** build em 2026-11-12 nas branches `feature/insumos` do pai, da Execução e da Cobrança (worktree `.claude/worktrees/insumos`); nada commitado. C1-C9 verdes na Execução (suíte completa 136 + 9 integração) e C10 verde na Cobrança (suíte completa 85); `dotnet format --verify-no-changes` limpo nas duas. Faltam: gates `proofs.md`/mutações, Verifier independente e `verification.md`
- **Decisões do builder (revisar):** consumidor de reserva na mesma fila de entrada da Execução (`Sqs:LiberadasQueueUrl`, despacho por `tipo`); respostas ao OS pela fila `os-service`; comando repetido de outra mensagem da mesma OS devolve de novo `insumos.reservados.v1` sem reservar (reserva toda cancelada devolve `insumos.reserva-falhou.v1` sem itens); `POST /api/v1/orcamentos` da Cobrança ganhou `itens` (`servico`/`insumo`, `referenciaId`, `quantidade`) e mantém `valor` direto; Cobrança lê preços por `GET /api/v1/servicos` e `/insumos` da Execução (listagem até 500 itens; config `Execucao:BaseUrl` e `Execucao:BearerToken`); seed de 5 serviços e 5 insumos na migration; saldo máximo por insumo 1.000.000.000
- **Abandoned:** none
- **Walked:** none - backend, sem tela
