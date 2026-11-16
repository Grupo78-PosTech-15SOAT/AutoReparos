# fase4/saga-compensacoes proofs

**HEAD**: 6f32542
**Tree**: clean
**Result**: 21 pass, 0 fail; mutations 6 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~AprovacaoReservaAntesDe` | count unknown | - | pass |
| C1 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~AprovacaoSemItensVaiPar` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~ReservadosLiberaExecuca` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~SemSaldoAguardaInsumos` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CompensaReservaEOrcamen` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~PrazoSemRespostaCompens` | count unknown | - | pass |
| C6 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CancelamentoAposConclus` | count unknown | - | pass |
| C13 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~OrcamentoAp` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~ComandosConflitantesRec` | count unknown | - | pass |
| C14 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CancelarReservaSoDepois` | count unknown | - | pass |
| C8 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~EventosConcorrentesGrav` | count unknown | - | pass |
| C9 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~PrazoTecnicoReenviaEEsg` | count unknown | - | pass |
| C10 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~LembreteNaoExpiraOrcame` | count unknown | - | pass |
| C11 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~ConsoleAdminRegistraAca` | count unknown | - | pass |
| C12 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~ReprocessaDlqDoOs` | count unknown | - | pass |
| C12 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~ReprocessaD` | count unknown | - | pass |
| C12 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~ReprocessaD` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.OS/AutoReparos.OS.slnx --verify-no-changes` | - | - | pass |
| Build | `dotnet build submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx --verify-no-changes` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| C9 limite de reenvios do passo < -> <= (quarto reenvio) | `submodules\AutoReparos.OS\AutoReparos.OS.Domain\SagaOS.cs:362` | yes |
| C5/C9 prazo da reserva usa PrazoLiberacao | `submodules\AutoReparos.OS\AutoReparos.OS.Domain\SagaOS.cs:344` | yes |
| C12 DLQ do OS deixa de rotear execucao.reservar-insumos.v1 para a fila da Execucao | `submodules\AutoReparos.OS\AutoReparos.OS.Api\ReprocessadorDlq.cs:44` | yes |
| C10 intervalo do lembrete de pagamento 24 h -> 12 h | `submodules\AutoReparos.OS\AutoReparos.OS.Domain\SagaOS.cs:22` | yes |
| C14 remove a trava D-33 (cancelamento aceito em ReservandoInsumos) | `submodules\AutoReparos.OS\AutoReparos.OS.Domain\SagaOS.cs:249` | yes |
| C4 reserva compensada depois do orcamento (ordem invertida) | `submodules\AutoReparos.OS\AutoReparos.OS.Domain\SagaOS.cs:617` | yes |
