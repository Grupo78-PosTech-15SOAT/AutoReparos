# fase4/os-service proofs

**HEAD**: 13eec18
**Tree**: clean
**Result**: 7 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CreateValidOrder` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~InvalidOrderReturns422` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~OrderHistoryIsChronolog` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Api.Tests --filter FullyQualifiedName~UnauthorizedOrderAc` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.IntegrationTests --filter FullyQualifiedName~ServiceDatab` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.OS/AutoReparos.OS.slnx --verify-no-changes` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| C4: papel Cliente passa a ser autorizado (403 vira acesso) | `submodules\AutoReparos.OS\AutoReparos.OS.Api\Program.cs:44` | yes |
| C3: historico em ordem cronologica inversa | `submodules\AutoReparos.OS\AutoReparos.OS.Api\Program.cs:68` | yes |
| C2: limite de 2000 caracteres da observacao deslocado em um (fronteira 2000 aceita) | `submodules\AutoReparos.OS\AutoReparos.OS.Api\CorpoJson.cs:55` | yes |
