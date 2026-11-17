# fase4/web-deploy proofs

**HEAD**: 79a92bc
**Tree**: clean
**Result**: 21 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base web-deploy` | 1 | - | pass |
| C1 | `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base roles-deploy` | 1 | - | pass |
| C2 | `python3 scripts/specs/simular_iam.py --caso web` | count unknown | - | pass |
| C3 | `python3 scripts/specs/assert_web_deploy.py --check plataforma-no-ar` | count unknown | - | pass |
| C4 | `python3 scripts/specs/assert_web_deploy.py --check chart` | count unknown | - | pass |
| C5 | `python3 scripts/specs/assert_web_deploy.py --check chart-config` | count unknown | - | pass |
| C6 | `python3 scripts/specs/assert_web_deploy.py --check deploy-job` | count unknown | - | pass |
| C7 | `python3 scripts/specs/assert_web_deploy.py --check deploy-get` | count unknown | - | pass |
| C8 | `python3 scripts/specs/assert_web_deploy.py --check deploy-condicao` | count unknown | - | pass |
| C9 | `python3 scripts/specs/assert_web_deploy.py --check deploy-concorrencia` | count unknown | - | pass |
| C10 | `python3 scripts/specs/assert_web_deploy.py --check environment` | count unknown | - | pass |
| C11 | `python3 scripts/specs/assert_web_deploy.py --check deploy-no-ar` | count unknown | - | pass |
| C12 | `python3 scripts/specs/assert_web_deploy.py --check quality-verde` | count unknown | - | pass |
| C13 | `python3 scripts/specs/assert_web_deploy.py --check guarda-chuva` | count unknown | - | pass |
| C14 | `python3 scripts/specs/assert_web_deploy.py --check pai-sem-web` | count unknown | - | pass |
| C15 | `python3 scripts/specs/assert_web_deploy.py --check destroy-web` | count unknown | - | pass |
| C16 | `python3 scripts/specs/assert_web_deploy.py --check rota-migrada` | count unknown | - | pass |
| C17 | `python3 scripts/specs/assert_web_deploy.py --check raiz-publica` | count unknown | - | pass |
| Build | `helm lint submodules/AutoReparos.Web/deploy/helm/web` | - | - | pass |
| Build | `helm lint submodules/AutoReparos.Infra.K8s/k8s -f submodules/AutoReparos.Infra.K8s/k8s/values.yaml -f submodul` | - | - | pass |
| Build | `terraform -chdir=submodules/AutoReparos.Infra.K8s/terraform/stacks/base fmt -check -recursive` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| C8/AC 7: deploy com always() e ignorar_quality deixa de exigir test verde | `submodules/AutoReparos.Web/.github/workflows/ci-cd.yml:97` | yes |
| C1/porta 2: role do Web ganha edição também no namespace autoreparos (observabilidade) | `submodules/AutoReparos.Infra.K8s/terraform/stacks/base/servicos.tf:189` | yes |
| C14/AC 10, porta 1: deploy-app do pai religa o subchart web com --set web.enabled=true | `.github/workflows/deploy-app.yml:125` | yes |
