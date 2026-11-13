# fase4/web proofs

**HEAD**: cf31a63
**Tree**: clean
**Result**: 12 pass, 0 fail; mutations 5 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `python scripts/specs/assert_web_extraction.py --check source` | count unknown | - | pass |
| C2 | `python scripts/specs/assert_web_extraction.py --check cadastros` | count unknown | - | pass |
| C3 | `python scripts/specs/assert_web_extraction.py --check gitlink` | count unknown | - | pass |
| C4 | `python scripts/specs/assert_web_extraction.py --check yarn` | 1 | - | pass |
| C5 | `python scripts/specs/assert_web_extraction.py --check docker` | count unknown | - | pass |
| C6 | `python scripts/specs/assert_web_extraction.py --check runtime-env` | count unknown | - | pass |
| C7 | `python scripts/specs/assert_web_extraction.py --check web-ci` | count unknown | - | pass |
| C8 | `python scripts/specs/assert_web_extraction.py --check parent-paths` | count unknown | - | pass |
| C9 | `python scripts/specs/assert_web_extraction.py --check cadastros-ci` | count unknown | - | pass |
| C10 | `python scripts/specs/assert_web_extraction.py --check readme` | count unknown | - | pass |
| Build | `npx --yes yarn@1.22.22 --cwd submodules/AutoReparos.Web build` | - | - | pass |
| Build | `dotnet build submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.slnx -m:1` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| C7: CI do Web deixa de disparar em push na develop | `submodules\AutoReparos.Web\.github\workflows\ci-cd.yml:7` | yes |
| C7: CI do Web dispara PR na develop em vez da main | `submodules\AutoReparos.Web\.github\workflows\ci-cd.yml:5` | yes |
| C5: rota /login deixa de cair no index.html (200 vira 404) | `submodules\AutoReparos.Web\nginx.conf:24` | yes |
| C5: servidor devolve HTML genérico 200 em vez do Angular | `submodules\AutoReparos.Web\nginx.conf:24` | yes |
| C6: URL com apóstrofo deixa de ser serializada antes de entrar no JavaScript | `submodules\AutoReparos.Web\docker-entrypoint.sh:17` | yes |
