# fase4/execucao proofs

**HEAD**: 15bad23
**Tree**: clean
**Result**: 8 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.Tests --filter FullyQualifiedName~ReleasedO` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.Tests --filter FullyQualifiedName~Execution` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.Tests --filter FullyQualifiedName~ManualRee` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.Tests --filter FullyQualifiedName~Duplicate` | count unknown | - | pass |
| C6 | `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.Tests --filter FullyQualifiedName~Dependenc` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.Execution/AutoReparos.Execution.slnx` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.Execution/AutoReparos.Execution.slnx --verify-no-changes` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| OS ja na fila deixa de impedir uma segunda entrada (condicao de deduplicacao por OS; ajustada apos a refatoracao da Saga) | `submodules\AutoReparos.Execution\AutoReparos.Execution.Application\Contratos.cs:99` | yes |
| reexecucao passa a ser aceita fora do estado Falhou (regra de estado da reexecucao) | `submodules\AutoReparos.Execution\AutoReparos.Execution.Domain\OrdemExecucao.cs:61` | yes |
| conclusao deixa de ser publicada para o Billing (so OS Service) | `submodules\AutoReparos.Execution\AutoReparos.Execution.Application\Contratos.cs:165` | yes |
