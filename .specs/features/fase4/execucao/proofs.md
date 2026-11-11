# fase4/execucao proofs

**HEAD**: 81815c0
**Tree**: clean
**Result**: 8 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~ReleasedOrd` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~ExecutionSt` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.IntegrationTests --filter FullyQualifiedName~` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~ManualReexe` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~DuplicateEv` | count unknown | - | pass |
| C6 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~DependencyR` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.Execucao/AutoReparos.Execucao.slnx` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.Execucao/AutoReparos.Execucao.slnx --verify-no-changes` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| OS ja na fila deixa de impedir uma segunda entrada (condicao de deduplicacao por OS; ajustada apos a refatoracao da Saga) | `submodules\AutoReparos.Execucao\AutoReparos.Execucao.Application\Contratos.cs:99` | yes |
| reexecucao passa a ser aceita fora do estado Falhou (regra de estado da reexecucao) | `submodules\AutoReparos.Execucao\AutoReparos.Execucao.Domain\OrdemExecucao.cs:61` | yes |
| conclusao deixa de ser publicada para o Billing (so OS Service) | `submodules\AutoReparos.Execucao\AutoReparos.Execucao.Application\Contratos.cs:164` | yes |
