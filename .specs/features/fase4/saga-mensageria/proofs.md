# fase4/saga-mensageria proofs

**HEAD**: 15bad23
**Tree**: clean
**Result**: 9 pass, 0 fail; mutations 0 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~SagaPersistsBeforePubli` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~SagaResumeAfterRestart` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~SagaDuplicateEvent` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CompensatesEachFailedSt` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CapturedPaymentForwardR` | count unknown | - | pass |
| C6 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~RetryExhaustionDeadLett` | count unknown | - | pass |
| C7 | `python submodules/AutoReparos.OS/scripts/assert_saga_readme.py --name saga-rationale --strategy orchestrated -` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.OS/AutoReparos.OS.slnx --verify-no-changes` | - | - | pass |
