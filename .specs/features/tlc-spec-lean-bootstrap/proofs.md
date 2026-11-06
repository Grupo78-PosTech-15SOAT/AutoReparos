# tlc-spec-lean-bootstrap proofs

**HEAD**: 005fa19
**Tree**: dirty - .agents/AGENTS.md, submodules/AutoReparos.App, submodules/AutoReparos.AuthLambda, submodules/AutoReparos.Infra.Database, submodules/AutoReparos.Infra.K8s
**Result**: 4 pass, 0 fail; mutations 0 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `walk - inspect .agents/AGENTS.md TLC workflow instructions` | - | - | pass (Walked) |
| C2 | `python .specs/tlc-spec-lean/scripts/validate_project_config.py tlc-spec-lean-bootstrap` | count unknown | - | pass |
| C3 | `python .specs/tlc-spec-lean/scripts/validate_checks.py tlc-spec-lean-bootstrap --root .` | count unknown | - | pass |
| C4 | `git diff --exit-code HEAD -- docs/specs/` | count unknown | - | pass |
