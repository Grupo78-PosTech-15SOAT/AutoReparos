# fase4/eventos-portugues proofs

**HEAD**: e078c04
**Tree**: clean
**Result**: 13 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --no-build --no-restore --filter FullyQualifiedName` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --no-build --no-restore --filter FullyQualifiedName` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --no-build --no-restore --filter FullyQualifiedName` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQu` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQu` | count unknown | - | pass |
| C6 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQu` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --no-build --no-restore --filter FullyQualifiedName` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQu` | count unknown | - | pass |
| C7 | `python scripts/specs/verificar_eventos_portugues.py` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx -m:1` | - | - | pass |
| Build | `dotnet build submodules/AutoReparos.Cobranca/AutoReparos.Billing.slnx -m:1` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.OS/AutoReparos.OS.slnx --verify-no-changes --no-restore` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.Cobranca/AutoReparos.Billing.slnx --verify-no-changes --no-restore` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| NormalizarComando deixa de converter o tipo legado billing.orcamento.cancelar.v1 para cobranca.cancelar-orcamento.v1 | `submodules\AutoReparos.OS\AutoReparos.OS.Domain\TiposMensagem.cs:24` | yes |
| ComandoSaga.NormalizarTipo atualiza apenas o registro Tipo e mantem o tipo legado no JSON Corpo enviado | `submodules\AutoReparos.OS\AutoReparos.OS.Domain\ComandoSaga.cs:83` | yes |
| EhCancelamentoOrcamento rejeita o alias legado billing.orcamento.cancelar.v1 em transito | `submodules\AutoReparos.Cobranca\AutoReparos.Billing.Application\Mensageria.cs:20` | yes |
