# fase4/cobranca proofs

**HEAD**: f246f06
**Tree**: clean
**Result**: 11 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQ` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQ` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQ` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQ` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQ` | count unknown | - | pass |
| C6 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQ` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQ` | count unknown | - | pass |
| C8 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQ` | count unknown | - | pass |
| C9 | `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --no-build --no-restore --filter FullyQ` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx -m:1` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx --verify-no-changes --no-restore` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| checkout do cliente deixa de validar o vinculo com o dono do orcamento (porta 4) | `submodules\AutoReparos.Cobranca\AutoReparos.Cobranca.Application\Contratos.cs:61` | yes |
| reembolso deixa de somar o ja reembolsado (limite cumulativo) | `submodules\AutoReparos.Cobranca\AutoReparos.Cobranca.Domain\Pagamento.cs:60` | yes |
| status approved do Mercado Pago mapeado para Rejeitado em vez de Capturado | `submodules\AutoReparos.Cobranca\AutoReparos.Cobranca.Infrastructure\Integracoes.cs:58` | yes |
