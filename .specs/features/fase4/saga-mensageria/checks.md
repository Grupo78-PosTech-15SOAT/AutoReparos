# Saga and messaging checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx`; `dotnet format submodules/AutoReparos.OS/AutoReparos.OS.slnx --verify-no-changes`
Plan: `.specs/features/fase4/saga-mensageria/plan.md`

7 checks · 1 slice · 2 one-way doors · 0 open questions, 0 blocking

## Checks

### S1 - Durable orchestration

**C1** - Saga state and correlation id are persisted before the first command is published (AC 1)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~SagaPersistsBeforePublish`

**C2** - Restart resumes a non-terminal Saga from its last persisted state without losing completed stages (AC 2)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~SagaResumeAfterRestart`

**C3** - Duplicate event id is deduplicated and causes no repeated business effect (AC 3)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~SagaDuplicateEvent`

**C4** - Each compensable stage failure executes its declared compensation and records outcome/error (AC 4)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CompensatesEachFailedStage`

**C5** - Captured payment sync failure retries forward and alerts without automatic refund (AC 5)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CapturedPaymentForwardRecovery`

**C6** - Exhausted bounded retry with exponential backoff and jitter routes the message to DLQ and emits an alert (AC 6)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~RetryExhaustionDeadLetter`

**C7** - README names persisted orchestration and justifies it against choreography (AC 7)
Proof: `python submodules/AutoReparos.OS/scripts/assert_saga_readme.py --name saga-rationale --strategy orchestrated --state-store DynamoDB`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| orchestration requirements (7) | persist-before-publish C1 · restart resume C2 · dedup C3 · compensation C4 · forward recovery C5 · DLQ/alert C6 · documented rationale C7 | - |
| Saga lifecycle cases (4) | initial C1 · non-terminal restart C2 · compensable failure C4 · captured payment failure C5 | - |

## Swept

- validation: C1, C4
- failure modes: C2, C4-C6
- idempotency: C3
- authorization: n/a - internal service messages carry service identity, detailed contract is owned by service plans
- concurrency: C1-C3
- data lifecycle: C1, C2
- dependency failure: C4-C6
- state transitions: C1-C5
- observability: C5, C6

## Handoff

- **Settled mid-build:** bounded retries, backoff, DLQ/alert, never repeat captured charge - user: "Adote limites padrão por dependência, backoff exponencial com jitter e DLQ/alerta ao esgotar; nunca repetir cobrança capturada."
- **Dispatched:** none
- **Boundary:** contrato de mensagens em `contracts.md`; build em andamento nos repositorios OS, Billing e Execution
- **Abandoned:** none
- **Walked:** none - backend behavior, no screen capture
