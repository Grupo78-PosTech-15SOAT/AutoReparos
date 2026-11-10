# TLC Spec Lean bootstrap checks

Mutations: 3
Build: none - process configuration only; no application code is changed
Plan: `.specs/features/tlc-spec-lean-bootstrap/plan.md`

## Checks

### S1 - Project workflow and artifacts

**C1** - New feature guidance requires approved plan, validated checks, build, and independent verification phases (AC 1, 5)
Proof: `walk` - inspect `.agents/AGENTS.md` TLC workflow instructions

**C2** - Project config declares `mutations: 3`, and scripts/templates are stored under `.specs/` (AC 2, 3)
Proof: `python .agents/skills/tlc-spec-lean/scripts/validate_project_config.py tlc-spec-lean-bootstrap`

**C3** - A plan without explicit user approval is rejected by the plan/check gate (AC 4)
Proof: `python .agents/skills/tlc-spec-lean/scripts/validate_checks.py tlc-spec-lean-bootstrap --root .`

**C4** - Existing `docs/specs/` content is preserved (AC 6)
Proof: `git diff --exit-code HEAD -- docs/specs/`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| workflow phases (4) | plan C1 · checks C1 · build C1 · verification C1 | - |
| project integration paths (3) | config C2 · scripts C2 · feature artifacts C2 | - |
| approved-plan gate (1) | explicit user approval C3 | - |
| historical spec scope (1) | docs/specs preserved C4 | - |

## Swept

- validation: C3
- failure modes: n/a - local documentation and validator installation only
- idempotency: n/a - no runtime behavior
- authorization: existing - approval line required by plan/check gate
- concurrency: n/a - no runtime state
- data lifecycle: n/a - no user data
- dependency failure: n/a - standard library scripts only
- state transitions: C1
- observability: n/a - no application runtime changes

## Handoff

- **Settled mid-build:** none
- **Dispatched:** none
- **Boundary:** bootstrap configuration and artifact structure added; proof gate unavailable because Python is not installed
- **Abandoned:** validator execution and TLC verification gate; Python runtime unavailable
- **Walked:** not applicable
