# fase4/web-adaptacao proofs

**HEAD**: a500dde
**Tree**: dirty - submodules/AutoReparos.Cadastros, submodules/AutoReparos.Cobranca, submodules/AutoReparos.Execucao, submodules/AutoReparos.Infra.K8s, submodules/AutoReparos.OS
**Result**: 22 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/config/a` | 1 | - | pass |
| C2 | `python3 scripts/specs/assert_web_adaptacao.py --check proxy` | count unknown | - | pass |
| C3 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/http/err` | 1 | - | pass |
| C4 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/http/jwt` | 1 | - | pass |
| C5 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orde` | 1 | - | pass |
| C6 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orde` | 1 | - | pass |
| C7 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orde` | 1 | - | pass |
| C8 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orde` | 1 | - | pass |
| C9 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/port` | 1 | - | pass |
| C10 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orca` | 1 | - | pass |
| C11 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/port` | 1 | - | pass |
| C12 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/paga` | 1 | - | pass |
| C13 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/exec` | 1 | - | pass |
| C14 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/insu` | 1 | - | pass |
| C14 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/serv` | 1 | - | pass |
| C15 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/saga` | 1 | - | pass |
| C16 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/saga` | 1 | - | pass |
| C17 | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/auth/rol` | 1 | - | pass |
| C18 | `python3 scripts/specs/assert_web_adaptacao.py --check legado` | count unknown | - | pass |
| C19 | `python3 scripts/specs/assert_web_adaptacao.py --check cobertura` | 17 | - | pass |
| C20 | `python3 scripts/specs/assert_web_adaptacao.py --check node` | count unknown | - | pass |
| Build | `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web build` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| exigeToken prefixo /api/v1/ -> qualquer /api/ (vaza token no login e legado) | `submodules/AutoReparos.Web/src/app/core/http/jwt.interceptor.ts:10` | yes |
| calculo do total do orcamento ignora quantidade do item | `submodules/AutoReparos.Web/src/app/features/orcamento/pages/orcamento-page/orcamento-page.component.ts:153` | yes |
| retentativa de saga aceita envio com motivo em branco | `submodules/AutoReparos.Web/src/app/features/sagas/pages/sagas-page/sagas-page.component.ts:271` | yes |
