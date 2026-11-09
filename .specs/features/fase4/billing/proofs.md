# fase4/billing proofs

**HEAD**: b739b91 (código e specs de telemetria OTel commitados; sem alterações pendentes nos arquivos do billing)
**Tree**: clean para os arquivos do billing
**Result**: 9 pass, 0 fail (C1–C9 + build/format); mutations 3 killed + 2 probes de C9 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test submodules/AutoReparos.Billing/AutoReparos.Billing.Tests --no-build --no-restore --filter FullyQualifiedName~CreatesBudgetForOrder` | 9 (suíte completa) | 9 | pass |
| C2 | `... --filter FullyQualifiedName~ChargeRequiresApprovalAndCompletion` | 9 | 9 | pass |
| C3 | `... --filter FullyQualifiedName~CreatesSingleCheckoutAttempt` | 9 | 9 | pass |
| C4 | `... --filter FullyQualifiedName~WebhookSignatureValidation` | 9 | 9 | pass |
| C5 | `... --filter FullyQualifiedName~ManualRetryAfterTerminalState` | 9 | 9 | pass |
| C6 | `... --filter FullyQualifiedName~CapturedPaymentForwardRecovery` | 9 | 9 | pass |
| C7 | `... --filter FullyQualifiedName~ApprovedRefundAudit` | 9 | 9 | pass |
| C8 | `... --filter FullyQualifiedName~DependencyRetryExhaustion` | 9 | 9 | pass |
| C9 | `... --filter FullyQualifiedName~TelemetryExports` | 1 | 1 | pass |
| Build | `dotnet build submodules/AutoReparos.Billing/AutoReparos.Billing.slnx -m:1` | - | - | pass (0 erros) |
| Build | `dotnet format submodules/AutoReparos.Billing/AutoReparos.Billing.slnx --verify-no-changes --no-restore` | - | - | pass (exit 0) |

Observação: o `--no-build` das linhas acima refere-se ao último build; a execução que gerou estes números rodou após `dotnet build` no HEAD b739b91.

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| checkout sem aprovação ou conclusão | `submodules\AutoReparos.Billing\AutoReparos.Billing.Application\Contratos.cs:47` | yes |
| reembolso acima do total pago | `submodules\AutoReparos.Billing\AutoReparos.Billing.Domain\Pagamento.cs:39` | yes |
| assinatura de webhook não validada | `submodules\AutoReparos.Billing\AutoReparos.Billing.Infrastructure\Integracoes.cs:95` | yes |

### Probes extras de C9 (fora de `mutations.json`, que segue com 3 por configuração)

| Probe | Location | Killed |
| --- | --- | --- |
| atributo `deployment.environment.name` trocado | `submodules\AutoReparos.Billing\AutoReparos.Billing.Api\TelemetriaBilling.cs` | yes (TelemetryExports falha) |
| instrumentação ASP.NET Core removida do tracing | `submodules\AutoReparos.Billing\AutoReparos.Billing.Api\Program.cs` | yes (TelemetryExports falha) |
