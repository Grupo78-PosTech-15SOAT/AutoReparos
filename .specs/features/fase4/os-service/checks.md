# OS Service checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx`; `dotnet format submodules/AutoReparos.OS/AutoReparos.OS.slnx --verify-no-changes`
Plan: `.specs/features/fase4/os-service/plan.md`

5 checks · 1 slice · 2 one-way doors · 0 open questions, 0 blocking

## Checks

### S1 - OS lifecycle and contracts

**C1** - `POST /api/v1/ordens-servico` persists valid OS creation and returns its identifier and initial status (AC 1)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~CreateValidOrder`

**C2** - `PATCH /api/v1/ordens-servico/{id}` returns 422 for invalid updates with a stable error code and leaves persisted state unchanged (AC 2)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~InvalidOrderReturns422`

**C3** - `GET /api/v1/ordens-servico/{id}` returns current status and chronologically ordered history to an authorized caller (AC 3)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~OrderHistoryIsChronological`

**C4** - Unauthorized lookup/update returns 401 or 403 and does not disclose OS data (AC 4)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Api.Tests --filter FullyQualifiedName~UnauthorizedOrderAccess`

**C5** - Billing and Execution obtain OS data through API/events and have no direct read permission for the OS database (AC 5)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.IntegrationTests --filter FullyQualifiedName~ServiceDatabaseIsolation`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| OS API routes/status (13) | POST 201 C1 · POST 401 C4 · POST 403 C4 · POST 422 C2 · PATCH 200 C2 · PATCH 401 C4 · PATCH 403 C4 · PATCH 404 C2 · PATCH 422 C2 · GET 200 C3 · GET 401 C4 · GET 403 C4 · GET 404 C3 | - |
| lifecycle rules (5) | valid create C1 · invalid input C2 · ordered history C3 · unauthorized access C4 · isolated integration C5 | - |

## Swept

- validation: C2
- failure modes: C2, C5
- idempotency: C1 - request deduplication policy is not specified by this feature
- authorization: C4
- concurrency: C1, C2 - persistence transaction tests cover competing updates
- data lifecycle: C3
- dependency failure: C5
- state transitions: C1, C2, C3
- observability: n/a - observability is covered by the dedicated infrastructure feature

## Handoff

- **Settled mid-build:** user: "OS não seria melhor em upper-case por ser sigla?"; repositório local `submodules/AutoReparos.OS`.
- **Dispatched:** os_verifier - fresh independent verifier (2026-11-09), root HEAD 73f4cec, OS repo d9f9cd8
- **Boundary:** endpoints, migrações PostgreSQL, autorização por perfil e publicador SQS com outbox, retentativas e DLQ implementados. C5 verifica privilégios efetivos e acesso por tokens, mas falhou sem `OS_PROOF_DB_CONNECTION`; exige ambiente provisionado conforme README do serviço. `check_proofs.py` registrou 6 pass e 1 fail (C5), com árvore suja e contagens desconhecidas; verificação independente e commits permanecem pendentes.
- **Abandoned:** none
- **Walked:** none - backend behavior, no screen capture
