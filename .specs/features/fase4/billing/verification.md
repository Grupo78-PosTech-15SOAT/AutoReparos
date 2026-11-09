# fase4/billing verification

**Verdict**: PASS (auto-verificação nesta rodada; verificador independente pendente)
**Mutations**: 3 em `mutations.json` - todas mortas; 2 probes extras de C9 - mortas
**Proofs**: `proofs.md` at `b739b91` (C1–C9, build e format passando)
**Verifier**: não despachado nesta rodada. As regras do projeto pedem Verifier independente (`author != verifier`), mas o agente não foi acionado automaticamente. Antes de fechar a feature, pedir revisão independente.

## Findings

| Finding | Evidence | Fix | Closed |
| --- | --- | --- | --- |
| Mutation probes were undetermined | The first attempt used `--no-build`, leaving the compiled test assembly unchanged during injection | Mutation proof commands now rebuild; all 3 mutants were killed | `cd98e9e` |
| C1 did not prove budget approval | Approval route was not exercised | C1 proves approval URL, anonymous/wrong-owner/owner cases, success, repeat conflict and not-found | `a0d4015` |
| C3 did not prove checkout URL or unknown ID | Response body and 404 were not asserted | C3 asserts the exact checkout URL and unknown payment 404; 401/403 are proven in C5 and 409 in C2 | `a0d4015` |
| C4 did not prove invalid HMAC leaves state unchanged | Only malformed signature was rejected | C4 submits a validly shaped wrong digest and asserts 401, unchanged Capturado state and no duplicate publish | `a0d4015` |
| C5 authorization coverage was overstated | Missing pending checkout authorization and not-found coverage | C5 proves 401 and 403 for pending checkout and authorized terminal retry | `a0d4015` |
| C7 did not prove full/cumulative refund boundaries | Only one partial refund was asserted | C7 proves duplicate reference has no second provider call, cumulative cap, exact completion and full refund | `cd98e9e` |
| C8 did not prove retry policy or queue exhaustion | Generic retry count omitted backoff and SQS behavior | C8 proves deterministic exponential+jitter delay/cap and five main sends, DLQ send and alert | `cd98e9e` |
| Duplicate refund reference could reach provider twice | Duplicate guard was absent from domain aggregate | `Pagamento.Reembolsar` rejects duplicate reference before provider invocation | `1fb5768` |
| Over-refund boundary was not tested | Upper bound was implemented but lacked boundary proof | C7 now rejects cumulative amount above 250 and allows exact 250 total | `cd98e9e` |
| OTel sem check nem teste (AC 9 adicionado em 2026-11-09) | Telemetria estava no `Program.cs` sem prova | C9 `TelemetryExports` prova service.name/deployment.environment.name no resource, spans de servidor, métrica `http.server.request.duration` e log via exportador in-memory; coletor inacessível (127.0.0.1:1) não impede resposta 401 | `fa1902d`, `b739b91` |

## Open items

- **Vulnerabilidade moderada (NU1902)** nos pacotes `OpenTelemetry.Api` 1.11.2 (GHSA-g94r-2vxg-569j) e `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.11.2 (GHSA-4625-4j76-fww9). Build passa, mas o aviso precisa ser avaliado e a versão atualizada quando houver patch.
- **Verifier independente** não executado nesta rodada (ver cabeçalho).
- **Escopo do OTel:** o processo de exportação não foi testado contra um coletor real; a prova cobre o pipeline in-memory e a tolerância a coletor indisponível.
