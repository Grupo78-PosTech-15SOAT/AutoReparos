# Billing checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx -m:1`; `dotnet format submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx --verify-no-changes --no-restore`
Plan: `.specs/features/fase4/cobranca/plan.md`

9 checks · 1 slice · 3 one-way doors · 0 open questions, 0 blocking

## Checks

### S1 - Billing lifecycle

**C1** - `POST /api/v1/orcamentos` persists an OS budget and its owner can approve it through `POST /api/v1/orcamentos/{id}/aprovacao` (AC 1)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQualifiedName~CreatesBudgetForOrder`

**C2** - Billing does not start a charge before budget approval and service completion (AC 2)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQualifiedName~ChargeRequiresApprovalAndCompletion`

**C3** - `POST /api/v1/pagamentos/{id}/checkout` creates exactly one active Checkout Pro attempt for eligible completed work (AC 3)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQualifiedName~CreatesSingleCheckoutAttempt`

**C4** - `POST /api/v1/webhooks/mercado-pago` updates persisted payment state and publishes a result for a valid signature; invalid signature is rejected (AC 4)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQualifiedName~WebhookSignatureValidation`

**C5** - Pending payment creates no second attempt; terminal rejection/cancel/expiry permits retry only through an authorized person (AC 5)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQualifiedName~ManualRetryAfterTerminalState`

**C6** - Captured charge is never retried; failed OS synchronization reconciles forward and alerts without automatic refund (AC 6)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQualifiedName~CapturedPaymentForwardRecovery`

**C7** - `POST /api/v1/pagamentos/{id}/reembolsos` records approver, amount and audit reference for an authorized total or partial refund (AC 7)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQualifiedName~ApprovedRefundAudit`

**C8** - Transient dependency failures use bounded retry with exponential backoff and jitter, then DLQ/alert on exhaustion (AC 8)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQualifiedName~DependencyRetryExhaustion`

**C9** - Billing API exports traces, metrics and logs via OTLP tagged with `service.name` and `deployment.environment.name`, and still answers requests when the OTLP collector is unreachable (AC 9)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQualifiedName~TelemetryExports`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| Billing routes/status (20) | budget POST 201 C1 · 401 C1 · 403 C1 · 422 C1 · approval POST 200 C1 · 401 C1 · 403 C1 · 404 C1 · 409 C1 · checkout POST 200 C3 · 401 C5 · 403 C5 · 404 C3 · 409 C2 · refund POST 201 C7 · 401 C7 · 403 C7 · 404 C7 · 409 C7 · 422 C7 | - |
| payment lifecycle examples (7) | no approval C2 · no completion C2 · eligible checkout C3 · duplicate delivery C4 · pending C5 · terminal retry C5 · captured recovery C6 | - |
| Webhook input classes (2) | valid signature C4 · invalid signature C4 | - |
| Telemetry signals (3) | traces C9 · metrics C9 · logs C9 | - |

## Swept

- validation: C4, C7
- failure modes: C6, C8
- idempotency: C3, C4, C6
- authorization: C5, C7
- concurrency: C3, C5
- data lifecycle: C1, C7
- dependency failure: C4, C6, C8
- state transitions: C2-C7
- observability: C6, C8, C9
- collector outage: C9

## Handoff

- **Settled mid-build:** bounded retries, backoff, DLQ/alert, never repeat captured charge - user: "Adote limites padrão por dependência, backoff exponencial com jitter e DLQ/alerta ao esgotar; nunca repetir cobrança capturada." Repository remains local with no remote URL - user: "Criar somente local por enquanto".
- **Dispatched:** billing_verifier - fresh independent verifier at HEAD 1fb5768; billing_verifier_r2 - fresh independent verifier (2026-11-09), Billing repo 26ebb6d; billing_verifier_r3 - segunda rodada independente (2026-11-09), root HEAD ff71be7; billing_verifier_r4 - terceira rodada independente (2026-11-09), root HEAD d982c59
- **Boundary:** C1-C8 implemented in `submodules/AutoReparos.Cobranca`; PostgreSQL schema/migration, authenticated Minimal APIs, Mercado Pago Checkout Pro/Webhooks, SQS results, retry/DLQ alerts, Dockerfile and CI included. No production resources or remote repository were created. `proofs.md` records the first green run at 1fb5768.
- **Abandoned:** none
- **Walked:** none - backend behavior proven through API and service tests; no screen capture
