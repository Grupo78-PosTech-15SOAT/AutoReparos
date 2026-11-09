# fase4/billing proofs

**HEAD**: cd98e9e
**Tree**: clean
**Result**: 10 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.Billing/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQua` | count unknown | - | pass |
| C2 | `dotnet test submodules/AutoReparos.Billing/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQua` | count unknown | - | pass |
| C3 | `dotnet test submodules/AutoReparos.Billing/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQua` | count unknown | - | pass |
| C4 | `dotnet test submodules/AutoReparos.Billing/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQua` | count unknown | - | pass |
| C5 | `dotnet test submodules/AutoReparos.Billing/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQua` | count unknown | - | pass |
| C6 | `dotnet test submodules/AutoReparos.Billing/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQua` | count unknown | - | pass |
| C7 | `dotnet test submodules/AutoReparos.Billing/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQua` | count unknown | - | pass |
| C8 | `dotnet test submodules/AutoReparos.Billing/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQua` | count unknown | - | pass |
| Build | `dotnet build submodules/AutoReparos.Billing/AutoReparos.Billing.slnx -m:1` | - | - | pass |
| Build | `dotnet format submodules/AutoReparos.Billing/AutoReparos.Billing.slnx --verify-no-changes --no-restore` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| checkout sem aprovação ou conclusão | `submodules\AutoReparos.Billing\AutoReparos.Billing.Application\Contratos.cs:47` | yes |
| reembolso acima do total pago | `submodules\AutoReparos.Billing\AutoReparos.Billing.Domain\Pagamento.cs:39` | yes |
| assinatura de webhook não validada | `submodules\AutoReparos.Billing\AutoReparos.Billing.Infrastructure\Integracoes.cs:95` | yes |
