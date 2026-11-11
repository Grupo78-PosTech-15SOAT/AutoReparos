# Infrastructure, CI/CD and observability checks

Mutations: 3
Build: `terraform -chdir=infra validate`; `helm lint charts/autoreparos`
Plan: `.specs/features/fase4/infra-ci-observabilidade/plan.md`

6 checks · 1 slice · 2 one-way doors · 0 open questions, 0 blocking

## Checks

### S1 - Delivery and operations

**C1** - PR pipeline for each service runs build, automated tests, quality analysis and fails when line coverage is below 80% (AC 1)
Proof: `python scripts/assert_pipeline.py --test-name pipeline-quality --service all --minimum-coverage 80`

**C2** - Main branch protection requires PR and successful automatic checks in all three service repositories (AC 2)
Proof: `python scripts/assert_branch_protection.py --test-name protected-main --repos os,billing,execution`

**C3** - Passing staging pipeline deploys the service to shared EKS staging namespace and records deployment result (AC 3)
Proof: `python scripts/assert_deployment.py --test-name staging-deploy --environment staging --cluster eks`

**C4** - Production deployment targets us-east-1 EKS and is blocked until release approval is recorded (AC 4)
Proof: `python scripts/assert_deployment.py --test-name production-approval --environment production --region us-east-1 --require-approval`

**C5** - Processed OS flow produces correlated logs, metrics and traces across the services in the Phase 3 stack (AC 5)
Proof: `dotnet test AutoReparos.Observability.Tests --filter FullyQualifiedName~CrossServiceCorrelation`

**C6** - Transient dependency failures use bounded retry/backoff with jitter and exhausted messages reach DLQ and alert (AC 6)
Proof: `dotnet test AutoReparos.Observability.Tests --filter FullyQualifiedName~RetryDlqAlert`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| pipeline/deployment requirements (6) | CI quality and coverage C1 · branch protection C2 · staging C3 · gated production C4 · correlated telemetry C5 · retry/DLQ alert C6 | - |
| environments (3) | local build/test C1 · staging EKS C3 · production us-east-1 after approval C4 | - |
| service repositories (3) | OS C1,C2 · Billing C1,C2 · Execution C1,C2 | - |

## Swept

- validation: C1, C2
- failure modes: C3-C6
- idempotency: C6
- authorization: C2, C4
- concurrency: C3
- data lifecycle: n/a - this feature provisions infrastructure and pipelines without business data changes
- dependency failure: C3, C5, C6
- state transitions: C3, C4
- observability: C5, C6

## Handoff

- **Settled mid-build:** user-approved local/staging/production targets and release approval for production
- **Dispatched:** none
- **Boundary:** checks derived from approved plan; build has not started
- **Abandoned:** none
- **Walked:** none - infrastructure and pipelines, no screen capture
