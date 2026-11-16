# fase4/insumos proofs

**HEAD**: 7a0c85f
**Tree**: clean
**Result**: 12 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~` | count unknown | - | pass |
| C6 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~ExecucaoCan` | count unknown | - | pass |
| C8 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~CatalogoExp` | count unknown | - | pass |
| C9 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~EntradaAume` | count unknown | - | pass |
| C10 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~OrcamentoGu` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.Execucao/AutoReparos.Execucao.slnx` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.Execucao/AutoReparos.Execucao.slnx --verify-no-changes` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| reserva desconta sem a guarda WHERE QuantidadeDisponivel >= q (porta 1, C2 e C4) | `submodules\AutoReparos.Execucao\AutoReparos.Execucao.Infrastructure\ExecucaoDbContext.cs:141` | yes |
| conclusao da execucao deixa de confirmar e baixar as reservas (C5) | `submodules\AutoReparos.Execucao\AutoReparos.Execucao.Application\Contratos.cs:173` | yes |
| valor do orcamento ignora a quantidade no total dos itens do snapshot (C10) | `submodules\AutoReparos.Cobranca\AutoReparos.Cobranca.Domain\Orcamento.cs:23` | yes |
