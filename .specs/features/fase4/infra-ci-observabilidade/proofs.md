# fase4/infra-ci-observabilidade proofs

**HEAD**: cd27ac4
**Tree**: dirty - scripts/specs/assert_observabilidade.py, submodules/AutoReparos.Cadastros, submodules/AutoReparos.Cobranca, submodules/AutoReparos.Execucao, submodules/AutoReparos.Infra.K8s
**Result**: 47 pass, 1 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TraceMensageriaTests` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~TraceMensag` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~TraceMensag` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TraceMensageriaTests` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~TraceMensag` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~TraceMensag` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TraceMensageriaTests` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~TraceMensag` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~TraceMensag` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Application.Tests --filter FullyQualifiedNa` | count unknown | - | pass |
| C6 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TraceMensageriaTests` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TraceMensageriaTests` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~TraceMensag` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~TraceMensag` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Application.Tests --filter FullyQualifiedNa` | count unknown | - | pass |
| C8 | `python3 scripts/specs/assert_observabilidade.py --check fontes` | count unknown | - | pass |
| C9 | `walk - rodar o checkout de ponta a ponta de deploy-servicos/smoke.md, abrir o Jaeger (kubectl port-forward svc` | - | - | pass (Walked) |
| C10 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TelemetriaSagaTests` | count unknown | - | pass |
| C11 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TelemetriaSagaTests` | count unknown | - | pass |
| C12 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TelemetriaSagaTests` | count unknown | - | pass |
| C13 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TelemetriaSagaTests` | count unknown | - | pass |
| C14 | `python3 scripts/specs/assert_observabilidade.py --check meter-os` | count unknown | - | pass |
| C14 | `python3 scripts/specs/simular_iam.py --caso irsa` | count unknown | - | pass |
| C15 | `python3 scripts/specs/assert_observabilidade.py --check grafana` | count unknown | - | pass |
| C16 | `walk - abrir o Grafana por port-forward depois do checkout de ponta a ponta, conferir o painel e o "Test" de c` | - | - | pass (Walked) |
| C17 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~PublicacaoS` | count unknown | - | pass |
| C18 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~PublicacaoS` | count unknown | - | pass |
| C19 | `scripts/specs/tf_test.sh submodules/AutoReparos.Execucao/infra/terraform sns` | 1 | - | pass |
| C19 | `python3 scripts/specs/assert_observabilidade.py --check sns-helm` | count unknown | - | pass |
| C20 | `scripts/specs/tf_test.sh submodules/AutoReparos.OS/infra/terraform assinatura-execucao` | 1 | - | pass |
| C20 | `scripts/specs/tf_test.sh submodules/AutoReparos.Cadastros/infra/terraform assinatura-execucao` | 1 | - | pass |
| C21 | `python3 scripts/specs/assert_observabilidade.py --check filtros-sns` | count unknown | - | pass |
| C22 | `python3 scripts/specs/simular_iam.py --caso sns` | count unknown | - | pass |
| C22 | `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base roles-deploy` | 1 | - | pass |
| C23 | `walk - concluir uma execução no checkout de ponta a ponta, conferir a Saga pela API do OS, o log do Cadastros,` | - | - | pass (Walked) |
| C24 | `python3 scripts/specs/assert_pipelines.py --check execucao-main` | count unknown | - | pass |
| C24 | `python3 scripts/specs/assert_pipelines.py --check cobertura` | count unknown | - | pass |
| C25 | `python3 scripts/specs/assert_branch_protection.py --check main` | count unknown | - | **fail** |
| C25 | `walk - abrir um PR de demonstração com um teste quebrado num repositório protegido, conferir o merge bloqueado` | - | - | pass (Walked) |
| C26 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter "FullyQualifiedName~SagaRetryTests|FullyQu` | count unknown | - | pass |
| C26 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~BillingMens` | count unknown | - | pass |
| C26 | `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~Resiliencia` | count unknown | - | pass |
| C26 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx` | - | - | pass |
| Build | `dotnet build submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx` | - | - | pass |
| Build | `dotnet build submodules/AutoReparos.Execucao/AutoReparos.Execucao.slnx` | - | - | pass |
| Build | `dotnet build submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.slnx` | - | - | pass |
| Build | `python3 scripts/specs/assert_deploy.py --check iac-formatado` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| C12 sagas_paradas conta as Sagas terminais em vez das não terminais | `submodules/AutoReparos.OS/AutoReparos.OS.Application/Saga/ColetorMetricasSaga.cs:20` | yes |
| C10 sagas_concluidas_total conta toda gravação em PagamentoCapturado, não só a transição | `submodules/AutoReparos.OS/AutoReparos.OS.Application/Saga/TelemetriaSaga.cs:33` | yes |
| C17 atributo tipo da publicação SNS leva o Destino em vez do tipo do envelope | `submodules/AutoReparos.Execucao/AutoReparos.Execucao.Infrastructure/Integracoes.cs:140` | yes |
