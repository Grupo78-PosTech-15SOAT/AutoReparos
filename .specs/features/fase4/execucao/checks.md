# Execution checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.Execution/AutoReparos.Execution.slnx`; `dotnet format submodules/AutoReparos.Execution/AutoReparos.Execution.slnx --verify-no-changes`
Plan: `.specs/features/fase4/execucao/plan.md`

6 checks · 1 slice · 2 one-way doors · 0 open questions, 0 blocking

## Checks

### S1 - Execution workflow

**C1** - `GET /api/v1/fila-execucao` exposes a released OS inserted exactly once into the execution queue (AC 1)
Proof: `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.Tests --filter FullyQualifiedName~ReleasedOrderQueuedOnce`

**C2** - `POST /api/v1/execucoes/{id}/etapas` persists diagnostic/repair stage, responsible mechanic and timestamp in execution history (AC 2)
Proof: `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.Tests --filter FullyQualifiedName~ExecutionStageHistory`

**C3** - Completed execution publishes completion to OS Service and Billing (AC 3)
Proof: `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.IntegrationTests --filter FullyQualifiedName~CompletionPublishedToConsumers`

**C4** - `POST /api/v1/execucoes/{id}/reexecucao` after failure requires intervention, preserves completed stages and permits manual re-execution after correction (AC 4)
Proof: `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.Tests --filter FullyQualifiedName~ManualReexecutionPreservesStages`

**C5** - Duplicate event id does not create a second queue item or execution stage (AC 5)
Proof: `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.Tests --filter FullyQualifiedName~DuplicateEventDeduplication`

**C6** - Transient database/messaging failure uses bounded retries with exponential backoff and jitter, then DLQ/alert on exhaustion (AC 6)
Proof: `dotnet test submodules/AutoReparos.Execution/AutoReparos.Execution.Tests --filter FullyQualifiedName~DependencyRetryExhaustion`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| Execution routes/status (13) | queue GET 200 C1 · 401 C1 · 403 C1 · stage POST 201 C2 · 401 C2 · 403 C2 · 404 C2 · 422 C2 · re-execution POST 202 C4 · 401 C4 · 403 C4 · 404 C4 · 409 C4 | - |
| execution scenarios (6) | released OS C1 · stage audit C2 · completion C3 · failure C4 · duplicate event C5 · dependency outage C6 | - |

## Swept

- validation: C2, C4
- failure modes: C4, C6
- idempotency: C1, C5
- authorization: C1, C2, C4
- concurrency: C1, C5
- data lifecycle: C2, C4
- dependency failure: C3, C6
- state transitions: C1-C4
- observability: C4, C6

## Handoff

- **Settled mid-build:** none
- **Dispatched:** execucao_verifier - fresh independent verifier (2026-11-09), root HEAD da5a92f, Execution repo 82d3b8e
- **Boundary:** build concluded in `submodules/AutoReparos.Execution` (new local git repo, branch `feature/execution-service`, no remote and not registered as submodule); C1-C6 proofs green locally; `check_proofs.py` gate and independent Verifier (`mutations.json`, `verification.md`) still pending
- **Abandoned:** none
- **Walked:** none - backend behavior, no screen capture
