# tlc-spec-lean-bootstrap proofs

**HEAD**: 30c4a7c
**Tree**: dirty - submodules/AutoReparos.App, submodules/AutoReparos.AuthLambda, submodules/AutoReparos.Infra.Database, submodules/AutoReparos.Infra.K8s
**Result**: 4 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `walk - inspect .agents/AGENTS.md TLC workflow instructions` | - | - | pass (Walked) |
| C2 | `python .agents/skills/tlc-spec-lean/scripts/validate_project_config.py tlc-spec-lean-bootstrap` | count unknown | - | pass |
| C3 | `python .agents/skills/tlc-spec-lean/scripts/validate_checks.py tlc-spec-lean-bootstrap --root .` | count unknown | - | pass |
| C4 | `git diff --exit-code HEAD -- docs/specs/` | count unknown | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| remove approval; checks validator must reject plan | `.specs\features\tlc-spec-lean-bootstrap\plan.md:3` | yes |
| project mutation count mismatch must fail config validation | `.agents\AGENTS.md:192` | yes |
| a required proof gate script is absent; project config proof must reject the package | `.agents\skills\tlc-spec-lean\scripts\validate_project_config.py:31` | yes |
