# Academic delivery package checks

Mutations: 3
Build: none - documentation and evidence packaging; no software build is produced by this feature
Plan: `.specs/features/fase4/pacote-entrega/plan.md`

6 checks · 1 slice · 1 one-way door · 0 open questions, 0 blocking

## Checks

### S1 - Evidence package

**C1** - Each service repository contains source, Dockerfile, Kubernetes manifests, CI/CD, coverage evidence in README, architecture docs and current Swagger or Postman collection (AC 1)
Proof: `python scripts/assert_delivery_contents.py --test-name all-required-files --repos os,billing,execution`

**C2** - Delivered video is public or unlisted on YouTube/Vimeo and duration is at most 15 minutes (AC 2)
Proof: walk - inspect delivered video platform, visibility and duration

**C3** - Video demonstrates full OS/Saga/failure flow, automated deployment of at least one service with passing tests, monitoring and distributed tracing (AC 3)
Proof: walk - review each required demonstration segment in the delivered video

**C4** - Final PDF contains participant identification, repository/video links, service/database/communication diagram, Saga strategy/rationale and service/technology rationale (AC 4)
Proof: `python scripts/assert_final_pdf.py --test-name required-sections --required-sections participants,repositories,video,architecture,saga,service-split,technology-rationale`

**C5** - Missing participants or final URLs are explicitly listed as pending and prevent submission (AC 5)
Proof: `python scripts/assert_package_readiness.py --test-name missing-inputs-block-submit --missing-inputs-block-submit`

**C6** - External submission is blocked until team review and explicit authorization are recorded (AC 6)
Proof: `python scripts/assert_package_readiness.py --test-name authorization-required --require-review --require-authorization`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| repository contents (7) | source C1 · Dockerfile C1 · Kubernetes manifests C1 · CI/CD C1 · coverage README C1 · architecture docs C1 · Swagger/Postman C1 | - |
| video requirements (6) | platform C2 · visibility C2 · <=15 minutes C2 · OS/Saga/failure C3 · deploy/tests C3 · monitoring/tracing C3 | - |
| final PDF content (7) | participant id C4 · repo links C4 · video link C4 · architecture diagram C4 · Saga rationale C4 · service split C4 · technology rationale C4 | - |
| submission safeguards (2) | missing inputs block C5 · review/authorization C6 | - |

## Swept

- validation: C1, C4, C5
- failure modes: C5, C6
- idempotency: n/a - generated artifacts are reviewed before a single external submission
- authorization: C6
- concurrency: n/a - evidence package is assembled as a single reviewed release
- data lifecycle: C1, C4
- dependency failure: C2-C4
- state transitions: C5, C6
- observability: C3

## Handoff

- **Settled mid-build:** none
- **Dispatched:** none
- **Boundary:** checks derived from approved plan; build has not started
- **Abandoned:** none
- **Walked:** video criteria require inspection of final delivered recording; none captured yet
