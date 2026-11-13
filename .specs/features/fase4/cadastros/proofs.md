# fase4/cadastros proofs

**HEAD**: ed35766
**Tree**: clean
**Result**: 21 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~AberturaValidaClienteEV` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.AuthLambda/tests --filter FullyQualifiedName~ClienteAuthFunctionTests` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.AuthLambda/tests --filter FullyQualifiedName~CadastrosClientTests` | count unknown | - | pass |
| C5 | `git grep -q -e "eks\]" -- .github/workflows/provision-aws.yml` | count unknown | - | pass |
| C5 | `git grep -q -e "eks\]" -- .github/workflows/destroy-aws.yml` | count unknown | - | pass |
| C5 | `grep -q ">= 1" submodules/AutoReparos.Infra.Database/terraform/variables.tf` | count unknown | - | pass |
| C6 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| C6 | `helm template autoreparos submodules/AutoReparos.Infra.K8s/k8s --show-only charts/api/templates/networkpolicy.` | count unknown | - | pass |
| C6 | `helm template autoreparos submodules/AutoReparos.Infra.K8s/k8s --set ingress.enabled=true --show-only template` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| C8 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| C8 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Application.Tests --filter FullyQualifiedNa` | count unknown | - | pass |
| C9 | `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedNam` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.slnx` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.slnx --verify-no-changes` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| C2 ConsultaCadastro deixa de aceitar o papel Mecanico | `submodules\AutoReparos.Cadastros\AutoReparos.Cadastros.Api\DependencyInjectionAPI.cs:82` | yes |
| C3 Cadastros deixa de validar o issuer do JWT | `submodules\AutoReparos.Cadastros\AutoReparos.Cadastros.Api\DependencyInjectionAPI.cs:63` | yes |
| C5 e-mail do cliente comparado sem ignorar caixa | `submodules\AutoReparos.Cadastros\AutoReparos.Cadastros.Application\Clientes\UseCases\AutenticarClienteUseCase.cs:19` | yes |
