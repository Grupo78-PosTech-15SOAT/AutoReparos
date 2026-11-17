# fase4/web-ui-polimento proofs

**HEAD**: 9132e76
**Tree**: dirty - submodules/AutoReparos.Web, .claude/
**Result**: 29 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/compon` | 1 | - | pass |
| C2 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/compon` | 1 | - | pass |
| C3 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/utils/` | 1 | - | pass |
| C4 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/pipes/` | 1 | - | pass |
| C5 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/utils/` | 1 | - | pass |
| C6 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/compon` | 1 | - | pass |
| C7 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/compon` | 1 | - | pass |
| C8 | `python3 scripts/specs/assert_web_ui.py --check estilos` | count unknown | - | pass |
| C9 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orde` | 1 | - | pass |
| C10 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orde` | 1 | - | pass |
| C11 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/saga` | 1 | - | pass |
| C12 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/saga` | 1 | - | pass |
| C13 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/saga` | 1 | - | pass |
| C14 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/saga` | 1 | - | pass |
| C15 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/exec` | 1 | - | pass |
| C16 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/exec` | 1 | - | pass |
| C17 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/exec` | 1 | - | pass |
| C18 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orca` | 1 | - | pass |
| C19 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orca` | 1 | - | pass |
| C20 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orca` | 1 | - | pass |
| C21 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/paga` | 1 | - | pass |
| C22 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/compon` | 1 | - | pass |
| C23 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orde` | 1 | - | pass |
| C24 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orde` | 1 | - | pass |
| C25 | `python3 scripts/specs/assert_web_ui.py --check home` | count unknown | - | pass |
| C26 | `python3 scripts/specs/assert_web_ui.py --check consistencia` | count unknown | - | pass |
| C27 | `python3 scripts/specs/assert_web_ui.py --check cobertura` | 35 | - | pass |
| C28 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orde` | 1 | - | pass |
| Build | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web build` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| C3 limite de 1 minuto deslocado (60 s exatos vira agora) | `submodules/AutoReparos.Web/src/app/shared/utils/data-hora.utils.ts:47` | yes |
| C9 sem deduplicacao dos ids de OS conhecidas | `submodules/AutoReparos.Web/src/app/features/ordens-servico/services/os-conhecidas.service.ts:146` | yes |
| C7 destruir aberto nao remove o painel do body | `submodules/AutoReparos.Web/src/app/shared/components/custom-select/custom-select.component.ts:411` | yes |
