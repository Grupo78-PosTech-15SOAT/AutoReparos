# Fase 4 - Tipos de mensagens em português verification

**Verdict**: PASS
**Mutations**: 3 - probed C2/C3 (`TiposMensagem.NormalizarComando`), C3 (`ComandoSaga.NormalizarTipo` no JSON `Corpo`) e C5 (`TiposMensagem.EhCancelamentoOrcamento`); todas as 3 mortas (`3 killed, 0 not`)
**Proofs**: `proofs.md` at e078c04 (13 pass, 0 fail; OS 45defd3, Cobrança 8e7bff7)
**Verifier**: independent sub-agent (author != verifier)

Critério AD-003: PASS exige nenhum achado ALTA ou MÉDIA aberto. As asserções de C1 a C7 foram inspecionadas contra os testes em `SagaFlowTests.cs`, `SagaCancelamentoRotaTests.cs`, `SagaPersistenceTests.cs`, `BillingMensageriaTests.cs` e `scripts/specs/verificar_eventos_portugues.py`:
- **C1**: `NewCancellationCommandUsesPortugueseTypeAndPreservesEnvelope` verifica `Etapa(1).Compensacao`, `ComandoSaga.Tipo`, `DestinoComando.Billing`, `EtapaCompensada = 1`, o conjunto exato das 7 chaves JSON (`correlacaoId`, `eventId`, `motivo`, `ocorreuEm`, `orcamentoId`, `ordemServicoId`, `tipo`), ausência do literal legado no payload e igualdade de valor dos 7 campos.
- **C2**: `LegacySagaStageGeneratesPortugueseCancellation` sobrescreve o documento bruto no DynamoDB com `billing.orcamento.cancelar.v1` na etapa 1, dispara a compensação e valida que `Etapas.Count` permanece 4, `Etapa(1).Compensacao` é normalizada para `cobranca.cancelar-orcamento.v1` e um único comando de compensação da etapa 1 é emitido com os 7 campos íntegros.
- **C3**: `LegacyPendingCancellationIsNormalizedWithoutChangingEventId` simula falha de envio, grava o comando pendente com o tipo antigo tanto em `ComandoSaga.Tipo` quanto em `Corpo.tipo`, comprova que o backoff (1,25s) e a ordem são respeitados antes do horário de retry e que, no vencimento, o despachador publica `cobranca.cancelar-orcamento.v1` mantendo o `eventId` original e persistindo o comando normalizado.
- **C4, C5, C6**: `PortugueseCancellationAppliesAndRecordsInbox`, `LegacyAndPortugueseAliasesDeduplicateByEventId` (testado nas duas ordens: legado → português e português → legado, verificando que `Outbox` tem apenas 1 resposta por orçamento e `EventosProcessados` tem 1 registro por `eventId`) e `UnknownCancellationTypeGoesToDeadLetter` (recusa `tipo_desconhecido` quando há IDs válidos e envio para `cmd-dlq` quando malformado) cobrem os caminhos de entrada na Cobrança; além disso, o helper padrão `CancelCommand` passou a usar `cobranca.cancelar-orcamento.v1`, exercitando também os testes existentes de recusa de negócio (`pagamento_existente`, `orcamento_recusado`), idempotência e concorrência com o novo tipo.
- **C7**: `verificar_eventos_portugues.py` inspeciona os 4 artefatos de contrato (catálogo transversal, `saga-mensageria/contracts.md`, `README.md` do OS e `README.md` da Cobrança), garantindo presença de `cobranca.cancelar-orcamento.v1` e exigindo marcação explícita de legado em qualquer linha que cite `billing.orcamento.cancelar.v1`.

Exemplos próprios avaliados no código em `HEAD`:
1. `ComandoSaga.NormalizarTipo()` faz parse do JSON (`JsonNode.Parse(Corpo) as JsonObject`) e altera exclusivamente a propriedade de topo `objeto["tipo"]` com `UnsafeRelaxedJsonEscaping`, de modo que um `motivo` contendo o texto literal `"billing.orcamento.cancelar.v1"` ou caracteres acentuados permanece intacto e comandos de outros tipos (`execucao.cancelar.v1`, `ordem-servico.liberada.v1`) não têm o JSON reescrito (`normalizado != atual` é falso).
2. `TiposMensagem.EhCancelamentoOrcamento` usa pattern matching ordinal exato (`tipo is CancelarOrcamento or CancelarOrcamentoLegado`), rejeitando variações de caixa (`COBRANCA.CANCELAR-ORCAMENTO.V1`), versão (`cobranca.cancelar-orcamento.v2`), híbridos (`cobranca.orcamento.cancelar.v1`) ou `null` como `tipo_desconhecido`.

## Findings

None - probed C1-C7 assertions, 3 mutations across OS (`TiposMensagem.cs`, `ComandoSaga.cs`) and Cobrança (`Mensageria.cs`), JSON field preservation in `ComandoSaga.NormalizarTipo()`, and dual-alias inbox deduplication in `CancelamentoOrcamentoService`.

## Open items

Itens de gravidade BAIXA e riscos conscientes aceitos pelo AD-003:

- **BAIXA - Comentário XML residual em `SagaOS.IniciarJaCancelada` (`SagaOS.cs:79`)**: o comentário doc-xml do método ainda menciona `<c>billing.orcamento.cancelar.v1</c>`, embora o método chame `Iniciar` + `IniciarCompensacao` e emita `cobranca.cancelar-orcamento.v1` (comprovado por `SagaCancelamentoRotaTests`).
- **BAIXA - Etapa 1 já em `SituacaoEtapa.Compensando` antes do upgrade mantém `Etapa(1).Compensacao` com o literal antigo caso expire por timeout**: `NormalizarCompensacao()` roda em `EmitirProximaCompensacao` (quando a etapa passa de `Concluida` para `Compensando`) e `NormalizarTipo()` roda em `ProximoComandoDevido` (para o comando pendente), mas se uma Saga já estava em `Compensando` na etapa 1 antes do deploy e vencer o prazo de 15 minutos sem resposta (`ExpirarCompensacao`), o texto do alerta `saga.compensacao-sem-resposta` lerá o valor antigo salvo em `Etapa(1).Compensacao`. Sem impacto funcional ou de contrato.
- **BAIXA - Constante `TiposMensagem.BillingOrcamentoCancelar` mantida no OS apontando para `CobrancaCancelarOrcamento`**: mantida como alias de código interno para compatibilidade binária/compilação; o valor da constante já é `"cobranca.cancelar-orcamento.v1"`.
