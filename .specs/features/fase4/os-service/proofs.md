# fase4/os-service proofs

**HEAD**: 4a3bff5
**Tree**: dirty - AGENTS.md, submodules/AutoReparos.App, submodules/AutoReparos.AuthLambda, submodules/AutoReparos.Billing/AutoReparos.Billing.Api/AutoReparos.Billing.Api.csproj, submodules/AutoReparos.Billing/AutoReparos.Billing.Api/Program.cs
**Result**: 7 pass, 0 fail; mutations 0 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CreateValidOrder` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~InvalidOrderReturns422` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~OrderHistoryIsChronolog` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Api.Tests --filter FullyQualifiedName~UnauthorizedOrderAc` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.IntegrationTests --filter FullyQualifiedName~ServiceDatab` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.OS/AutoReparos.OS.slnx --verify-no-changes` | - | - | pass |
