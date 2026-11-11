# Tipos de mensagens em português - checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx -m:1`; `dotnet build submodules/AutoReparos.Cobranca/AutoReparos.Billing.slnx -m:1`; `dotnet format submodules/AutoReparos.OS/AutoReparos.OS.slnx --verify-no-changes --no-restore`; `dotnet format submodules/AutoReparos.Cobranca/AutoReparos.Billing.slnx --verify-no-changes --no-restore`
Plan: `.specs/features/fase4/eventos-portugues/plan.md`

7 checks · 1 slice · 1 one-way door · 0 open questions, 0 blocking

## Checks

### S1 - Compensação com tipo em português

**C1** - Uma compensação nova persiste e publica `cobranca.cancelar-orcamento.v1` com os seis campos restantes do envelope inalterados (AC 1)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --no-build --no-restore --filter FullyQualifiedName~NewCancellationCommandUsesPortugueseTypeAndPreservesEnvelope`

**C2** - Etapa de Saga persistida com `billing.orcamento.cancelar.v1` cria uma única compensação com o tipo português (AC 2)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --no-build --no-restore --filter FullyQualifiedName~LegacySagaStageGeneratesPortugueseCancellation`

**C3** - Comando pendente com tipo antigo no registro e no JSON é enviado com tipo português e o mesmo `eventId`, demais campos, retry e ordem (AC 3)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --no-build --no-restore --filter FullyQualifiedName~LegacyPendingCancellationIsNormalizedWithoutChangingEventId`

**C4** - Cobrança processa o tipo português com a regra atual e registra `eventId` na Inbox na transação do cancelamento (AC 4)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQualifiedName~PortugueseCancellationAppliesAndRecordsInbox`

**C5** - Tipo legado mantém o mesmo efeito; dois envelopes com o mesmo `eventId` e nomes diferentes geram um único cancelamento e uma única resposta de negócio (AC 5)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQualifiedName~LegacyAndPortugueseAliasesDeduplicateByEventId`

**C6** - Tipo desconhecido `billing.orcamento.apagar.v1` permanece rejeitado com encaminhamento à DLQ (AC 6)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQualifiedName~UnknownCancellationTypeGoesToDeadLetter`

**C7** - Testes de contrato de OS e Cobrança exigem o novo tipo na saída; catálogo e READMEs o nomeiam e marcam o tipo antigo apenas como entrada legada (AC 7)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --no-build --no-restore --filter FullyQualifiedName~NewCancellationCommandUsesPortugueseTypeAndPreservesEnvelope`
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQualifiedName~PortugueseCancellationAppliesAndRecordsInbox`
Proof: `python scripts/specs/verificar_eventos_portugues.py`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| contrato do comando (7 campos) | `tipo` C1 · `eventId` C1,C3 · `correlacaoId` C1,C3 · `ordemServicoId` C1,C3 · `orcamentoId` C1,C3 · `ocorreuEm` C1,C3 · `motivo` C1,C3 | - |
| origem da mensagem (3) | compensação nova C1 · etapa antiga persistida C2 · comando antigo pendente C3 | - |
| tipo recebido pela Cobrança (3) | português C4 · legado C5 · desconhecido C6 | - |
| duplicação entre aliases (2) | novo depois do antigo C5 · antigo depois do novo C5 | - |
| documentação do contrato (4) | catálogo C7 · contrato da Saga C7 · README OS C7 · README Cobrança C7 | - |
| porta de contrato (1) | `cobranca.cancelar-orcamento.v1` emitido C1,C3 e aceito C4; alias legado aceito C5 | - |

## Swept

- validation: C4, C6
- failure modes: C3, C6
- idempotency: C5
- authorization: n/a - comando interno usa as autorizações já existentes dos serviços; nenhuma rota muda
- concurrency: C5 - a Inbox deduplica pelo mesmo `eventId`, inclusive com nomes diferentes
- data lifecycle: C2, C3, C5 - dados legados em DynamoDB, SQS e DLQ
- dependency failure: C3 - retry e ordem do comando pendente permanecem
- state transitions: C2, C4, C5
- observability: C6 - tipo inválido continua indo à DLQ

## Handoff

- **Settled mid-build:** user: "Pode seguir" (10/11/2026), aprovação do `plan.md`.
- **Dispatched:** none
- **Boundary:** implementação concluída nos repositórios OS e Cobrança, documentação e script verificador criados; pronto para rodar `check_proofs.py` e despachar o Verifier independente.
- **Abandoned:** none
- **Walked:** none - mudança de contrato de backend, sem tela.
