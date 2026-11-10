# Fase 4 - Saga e mensageria verification

**Verdict**: PASS
**Mutations**: 3 - C4 (ordem inversa das compensacoes), C1 (condicao de versao no DynamoDB), C6 (limite de 5 tentativas antes da DLQ); definidas em `mutations.json` e executadas pelo gate: 3 mortas, 0 sobreviventes
**Proofs**: `proofs.md` at 75b086b (9 pass, 0 fail; OS d8e8503, Billing a0f4870, Execution fb56593). Regressao no mesmo HEAD: os-service 7/7, billing 11/11, execucao 8/8, cada um com 3 mutacoes mortas
**Verifier**: independent sub-agent da 2a rodada (author != verifier); a 1a rodada foi FAIL

Criterio AD-003: PASS exige nenhum achado ALTA ou MEDIA aberto. A 2a rodada leu o codigo corrigido e os testes e conferiu o comportamento: o GET real da OS alimenta o cliente de readiness do Billing, a rota de cancelamento, o prazo da compensacao, os sete status de pagamento e o orcamento aprovado que chega apos o cancelamento (`d8e8503`, OS permanece `Cancelada`). Os checks C1 a C7 apontam para filtros que casam com testes reais, com asseracoes de estado persistido, ordem de envio, alertas e historico da OS. O sensor de mutacao cobre C1, C4 e C6.

## Findings

| Finding | Evidence | Fix | Closed |
| --- | --- | --- | --- |
| ALTA - Status da OS apos a execucao era ServicoConcluido, mas o Billing so libera a cobranca com Concluida; a Saga nunca chegava a PagamentoCapturado | `SagaContratoEntreServicosTests` usa o GET real da OS e exige status Concluida; `BillingContratoOsTests` (3 testes) mostra que so Concluida libera o checkout; `ServicoConcluido` nao existe mais no codigo | Vocabulario alinhado para Concluida (AD-011); testes de contrato nos dois repositorios | OS 96cac40, 56ac092; Billing a0f4870 |
| MEDIA - Compensacao sem gatilho de producao: ninguem publicava o cancelamento | `POST /api/v1/ordens-servico/{id}/cancelamento` com politica Administrador ou Atendente; `SagaCancelamentoRotaTests` cobre 404, 422, cancelamento sem Saga, etapas em ordem inversa, liberada nunca enviada e 409 apos conclusao ou pagamento | Rota de cancelamento (AD-011) | OS 96cac40, 56ac092, a1bece8 |
| MEDIA - Billing publicava pagamento com status Pendente e o OS o rejeitava como invalido (DLQ e alerta em evento normal) | Theory `SagaTransitions_NonCapturedPaymentResultOnlyRecordsHistory` cobre os 6 status nao capturados, sem alerta e sem mudar de etapa; valor desconhecido continua indo a DLQ | OS aceita os 7 status do Billing | OS 96cac40, 56ac092 |
| MEDIA - Saga presa em Compensando sem prazo quando o participante nao responde | `...CompensationWithoutConfirmationFailsAtTheDeadlineWithAnAlert`: 14min59s sem efeito, 15min leva a CompensacaoFalhou com alerta `saga.compensacao-sem-resposta`; confirmacao tardia rejeitada; prazo configuravel em `Saga:PrazoCompensacao` | Prazo persistido e verificado pelo despachador | OS 96cac40, 56ac092 |
| BAIXA - Cancelar em IntervencaoRequerida era recusado pelo Execution | `ExecutionCancellation_CancelsFailedExecutionKeepingStagesAndBlocksReexecution` | Execution cancela tambem em Falhou | Execution fb56593 |
| BAIXA - Cancelamento ultrapassava liberada pendente em backoff | `CancellationCommandNeverOvertakesAPendingCommandInBackoff` | Comandos despachados em ordem por Saga | OS 96cac40, 56ac092 |
| BAIXA - Uma excecao abortava o ciclo do despachador; consumidor podia estourar o visibility timeout | `ExceptionInOneSagaDoesNotAbortTheDispatcherCycle...`; `SagaConsumidorTests` (5 testes) | try/catch por Saga com alerta `saga.despacho-falhou`; extensao da visibilidade | OS 96cac40, 56ac092 |
| BAIXA - C7 provava palavras-chave, nao a justificativa | `assert_saga_readme.py` exige subsecao de justificativa, minimo de palavras e ao menos 3 de 4 criterios; `test_assert_saga_readme.py` | Verificador do README endurecido | OS a1bece8 |
| BAIXA - Asseracoes fracas e sem payload real | `SagaPersistsBeforePublish_EveryCommandOfTheCancellationFlowIsStoredBeforeItIsSent`; testes de contrato com literais de referencia nos dois repositorios | Persistencia verificada antes de cada envio; payloads de referencia | OS 56ac092; Billing a0f4870 |
| BAIXA - Orcamento aprovado que chega apos o cancelamento da OS reativava a OS (risco apontado pelo autor) | `ApprovalArrivingAfterCancellationCompensatesTheBudgetWithoutReactivatingTheOrder`: OS segue Cancelada, envia `billing.orcamento.cancelar.v1`, nao envia liberada, alerta `saga.orcamento-aprovado-apos-cancelamento` | Saga nasce compensando quando a OS ja esta Cancelada | OS d8e8503 |

## Open items

Achados BAIXA e riscos conscientes, aceitos pelo AD-003:

- **BAIXA (parcial) - `motivo` limitado a 2000 caracteres e validado, mas a lista `Registros` da Saga nao tem limite e o item DynamoDB tem teto de 400 KB** - Evidencia: `MensagemSaga.MotivoValido`, `DynamoDbSagaRepositorio` (documento em um unico atributo)
- **BAIXA (parcial) - OS confere `correlacaoId` e `orcamentoId` das confirmacoes (alerta `saga.mensagem-divergente`), mas o Billing ainda nao envia `correlacaoId` em `pagamento.resultado.v1` e o OS tolera a ausencia** - Evidencia: Billing `Contratos.cs` (`EventoPagamento`)
- **BAIXA (novo, N-1) - Corrida de milissegundos entre `orcamento.aprovado.v1` e a rota de cancelamento: a Saga le o status da OS e so depois grava; se a rota cancelar a OS nessa janela, a OS pode ser reativada** - Evidencia: `OrdemServico.cs:30` (`DefinirStatus` nao protege a transicao a partir de Cancelada), `OrquestradorSaga.cs`. Mitigacao possivel: guardar a transicao na sincronizacao
- **BAIXA (novo, N-2) - Literais de contrato duplicados entre OS e Billing em vez de artefato compartilhado; uma divergencia de um lado so passaria nos dois repositorios** - Evidencia: `SagaContratoEntreServicosTests`, `BillingContratoOsTests`
- **BAIXA (novo, N-3) - Sem tratamento para linhas antigas gravadas com `ServicoConcluido`** - Inocuo hoje: a Saga nunca o gravou nem foi para producao
- Mensagens fora de ordem rejeitadas (por exemplo `pagamento.resultado.v1` Capturado antes de `execucao.concluida.v1`) sao registradas como processadas e nunca reavaliadas; a checagem de prontidao do Billing barra o caso em condicoes normais
- `liberada` esgotada na DLQ deixa a Saga em `ExecucaoLiberada` sem reenvio; ha alerta `saga.comando-esgotado`, e o contrato declara intervencao humana
- Alertas sao logs de erro (`AlertadorLog.cs`), sem roteamento real (feature `infra-ci-observabilidade`); `saga.sincronizacao-os-falhou` repete a cada tentativa ate o teto de 5 minutos
- O sensor de mutacao cobriu C1, C4 e C6; C2, C3, C5 e C7 ficam sem mutacao (`mutations: 3`)
- `proofs.md` mostra "count unknown" na contagem de testes por filtro; o gate nao verifica quantos testes rodaram
- `mutate.py` da skill quebra no Windows (`signal.SIGHUP`) e o `check_proofs.py` sai com 0 mostrando 0 mutacoes; as mutacoes desta rodada rodaram com um shim local `sitecustomize.py` fora do repositorio
- Provisionamento real de DynamoDB, SQS e TTL fica para a feature de infraestrutura
