# fase4/execucao proofs

**HEAD**: da5a92f
**Tree**: clean
**Result**: 8 pass, 0 fail; mutations 0 killed, 0 not

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
