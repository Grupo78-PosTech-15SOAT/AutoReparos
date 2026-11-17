# Pacote acadêmico de entrega - verificação

**Verdict**: FAIL
**Mutations**: 4 - probed C4 (ordem inversa da compensação, pular a reserva sem insumos, pagamento recusado só no histórico, sincronização `Concluida`)
**Proofs**: `proofs.md` at 22de2d1
**Verifier**: independent sub-agent (author != verifier)

Escopo: `b0229df..22de2d1` no pai (`scripts/specs/assert_entrega.py`) e `origin/develop..cd0e421` em `submodules/AutoReparos.OS` (`AutoReparos.OS.BddTests`); código de produção sondado: `AutoReparos.OS.Domain/SagaOS.cs`.

Exemplos próprios, rodados como cenários descartáveis (não commitados, removidos) na suíte BDD em 22de2d1, ambos verdes: (a) pagamento `Expirado` seguido de `Capturado` numa OS sem insumos - a Saga fica em `ExecucaoConcluida` sem compensação e depois conclui em `PagamentoCapturado`/`Paga`; (b) cancelamento de OS sem insumos já liberada - compensa `execucao.cancelar.v1, cobranca.cancelar-orcamento.v1`, sem `execucao.cancelar-reserva.v1`, e termina `Compensada`/`Cancelada`. Nenhum achado veio deles.

## Findings

| Finding | Evidence | Fix | Closed |
| --- | --- | --- | --- |
| O AC 8 (3) diz que o pagamento recusado "só registra no histórico", mas nenhum cenário verifica o registro: apagar o registro passa | mutação `pagamento recusado não é registrado no histórico da Saga` (`SagaOS.cs:230`, `return Aceitar(...)` -> `return Transicao.Ok`) não morta | expor os registros da Saga no `ContextoSaga` e acrescentar ao cenário de pagamento recusado um passo que exija um registro `pagamento.resultado.v1` com o status `Rejeitado` | `803c4c0` (OS `8b011e6`) |
| No cenário de pagamento recusado, "nenhum comando de compensação foi enviado" só filtra tipos que contêm `cancelar`; qualquer outro comando despachado pela recusa (ex.: um novo `execucao.reservar-insumos.v1`) passa, e o AC exige que a recusa "só" registre | `PassosSaga.cs` `EntaoSemCompensacao` sobre `ContextoSaga.Compensacoes` (`Where(t => t.Contains("cancelar"))`) | no cenário (3), exigir também a lista exata de comandos enviados (`execucao.reservar-insumos.v1, ordem-servico.liberada.v1`), como o cenário (1) faz | `803c4c0` (OS `8b011e6`) |
| A prova de C4 conta testes aprovados, não os seis cenários nomeados: trocar um dos seis por qualquer outro cenário verde mantém a prova verde | `scripts/specs/assert_entrega.py` `checar_bdd` - só `Failed: 0` e `Passed >= 6` | conferir na saída do `dotnet test` (logger `console;verbosity=normal` ou trx) os títulos dos seis cenários do AC 8, ou ao menos os nomes esperados nos `.feature` | `803c4c0` (OS `8b011e6`) |
