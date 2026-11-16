# fase4/deploy-servicos proofs

**HEAD**: 33730bf
**Tree**: clean
**Result**: 53 pass, 0 fail; mutations 6 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `python3 scripts/specs/assert_deploy.py --check dockerfile-os` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Api.Tests --filter FullyQualifiedName~SaudeEndpointTests` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~SaudeEndpoi` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~SaudeEndpoi` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Api.Tests --filter FullyQualifiedName~MigrarESairTests` | count unknown | - | pass |
| C6 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~MigrarESair` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~MigrarESair` | count unknown | - | pass |
| C8 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~MigracaoNaI` | count unknown | - | pass |
| C9 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~MigracaoNaI` | count unknown | - | pass |
| C10 | `python3 scripts/specs/assert_deploy.py --check terraform-servicos` | count unknown | - | pass |
| C11 | `scripts/specs/tf_test.sh submodules/AutoReparos.OS/infra/terraform recursos` | 1 | - | pass |
| C11 | `scripts/specs/tf_test.sh submodules/AutoReparos.Cobranca/infra/terraform recursos` | 1 | - | pass |
| C11 | `scripts/specs/tf_test.sh submodules/AutoReparos.Execucao/infra/terraform recursos` | 1 | - | pass |
| C11 | `scripts/specs/tf_test.sh submodules/AutoReparos.Cadastros/infra/terraform recursos` | 1 | - | pass |
| C12 | `scripts/specs/tf_test.sh submodules/AutoReparos.OS/infra/terraform dynamodb` | 1 | - | pass |
| C13 | `python3 scripts/specs/simular_iam.py --caso irsa` | count unknown | - | pass |
| C14 | `python3 scripts/specs/assert_deploy.py --check chart-hooks` | count unknown | - | pass |
| C15 | `python3 scripts/specs/assert_deploy.py --check chart-deployment` | count unknown | - | pass |
| C16 | `python3 scripts/specs/assert_deploy.py --check chart-ingress` | count unknown | - | pass |
| C17 | `python3 scripts/specs/assert_deploy.py --check sem-admin` | count unknown | - | pass |
| C18 | `python3 scripts/specs/assert_pipelines.py --check deploy-producao` | count unknown | - | pass |
| C19 | `python3 scripts/specs/assert_pipelines.py --check deploy-verifica-saude` | count unknown | - | pass |
| C20 | `python3 scripts/specs/assert_pipelines.py --check sem-deploy-fora-da-main` | count unknown | - | pass |
| C21 | `python3 scripts/specs/assert_deploy.py --check sem-staging` | count unknown | - | pass |
| C22 | `python3 scripts/specs/assert_deploy.py --check plataforma-nos` | count unknown | - | pass |
| C23 | `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base roles-deploy` | 1 | - | pass |
| C24 | `python3 scripts/specs/simular_iam.py --caso deploy` | count unknown | - | pass |
| C25 | `python3 scripts/specs/assert_deploy.py --check pai-so-web` | count unknown | - | pass |
| C26 | `python3 scripts/specs/assert_deploy.py --check web-ingress` | count unknown | - | pass |
| C27 | `python3 scripts/specs/assert_deploy.py --check ssm-plataforma` | count unknown | - | pass |
| C28 | `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.Database/terraform provisionador` | 3 | - | pass |
| C29 | `python3 -m unittest discover -s submodules/AutoReparos.Infra.Database/lambda/provisionador/tests -p test_provi` | 7 | - | pass |
| C30 | `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base vpc-endpoints` | 1 | - | pass |
| C31 | `scripts/infra/configurar-github.sh --verificar` | count unknown | - | pass |
| C32 | `scripts/infra/configurar-github.sh --verificar-idempotencia` | count unknown | - | pass |
| C33 | `scripts/smoke/deploy-execucao.sh execucao` | count unknown | - | pass |
| C34 | `scripts/smoke/rollback-execucao.sh execucao` | count unknown | - | pass |
| C35 | `python3 scripts/specs/assert_deploy.py --check filas-produtor-consumidor` | count unknown | - | pass |
| C36 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Application.Tests --filter FullyQualifiedNa` | count unknown | - | pass |
| C36 | `python3 scripts/specs/assert_deploy.py --check jwt-audiencia` | count unknown | - | pass |
| C37 | `python3 -m unittest discover -s scripts/infra/tests -p test_token_servico.py` | 4 | - | pass |
| C37 | `scripts/infra/configurar-github.sh --verificar-token-servico` | count unknown | - | pass |
| C38 | `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base roles-deploy` | 1 | - | pass |
| C39 | `python3 scripts/specs/simular_iam.py --caso boundary` | count unknown | - | pass |
| C40 | `python3 scripts/specs/simular_iam.py --caso state` | count unknown | - | pass |
| C41 | `scripts/smoke/mercado-pago.sh` | count unknown | - | pass |
| C42 | `scripts/smoke/aws-credenciais.sh` | count unknown | - | pass |
| C43 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Application.Tests --filter FullyQualifiedNa` | count unknown | - | pass |
| C44 | `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base auth-lambda-rede` | 1 | - | pass |
| Build | `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx` | - | - | pass |
| Build | `dotnet build submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx` | - | - | pass |
| Build | `dotnet build submodules/AutoReparos.Execucao/AutoReparos.Execucao.slnx` | - | - | pass |
| Build | `python3 scripts/specs/assert_deploy.py --check iac-formatado` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| C18/AC 15 (D-60): deploy do Cadastros com always() deixa de exigir test verde | `submodules/AutoReparos.Cadastros/.github/workflows/ci-cd.yml:109` | yes |
| C24/porta 2-3: role de deploy lê e grava o state de qualquer serviço | `submodules/AutoReparos.Infra.K8s/terraform/stacks/base/politicas/deploy-servico.json.tftpl:188` | yes |
| C35/porta 4: OS publica comandos da Saga na fila sem consumidor da Cobrança | `submodules/AutoReparos.OS/deploy/helm/os/values.yaml:31` | yes |
| R2 C18/AC 15 (D-60): always() trocado por (success() \ | `\` | yes |
| R2 C39/porta 2: deny da tag Servico com ForAllValues (TagRole com Servico=os junto de outra chave passa) | `submodules/AutoReparos.Infra.K8s/terraform/stacks/base/politicas/deploy-servico.json.tftpl:138` | yes |
| R2 C24/C39 porta 2: role de deploy altera trust, política e tags das roles de outro serviço | `submodules/AutoReparos.Infra.K8s/terraform/stacks/base/politicas/deploy-servico.json.tftpl:57` | yes |
