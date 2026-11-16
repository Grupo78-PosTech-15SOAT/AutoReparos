# Registro dos smoke tests - deploy-servicos

Uma linha por execução contra o ambiente de produção (conta `198903849859`). As linhas automáticas são acrescentadas por [`scripts/smoke/rodar.sh`](../../../../scripts/smoke/rodar.sh); os testes manuais do [README](../../../../scripts/smoke/README.md#testes-manuais) entram à mão, com quem rodou.

| Data (UTC) | Commit | Teste | Resultado | Evidência |
| --- | --- | --- | --- | --- |
| 2026-11-16T05:20Z | `cbad386` | `deploy-execucao` | PASS | OK smoke_deploy execucao: pipeline verde, 200 com token do Cadastros e 401 sem token (registrado à mão, antes do `rodar.sh`) |
| 2026-11-16T05:25Z | `cbad386` | `rollback-execucao` | PASS | revisão 2 `failed`, revisão 3 `deployed` "Rollback to 1"; sem token 401, com token 200 (registrado à mão) |
| 2026-11-16T06:30Z | `490dee1` | `mercado-pago` | PASS | OK smoke_mercado_pago: credenciais no pod da Cobrança e webhook recusando assinatura inválida (registrado à mão) |
| 2026-11-16T06:32Z | `490dee1` | Access Token do Mercado Pago (manual, `GET /users/me`) | PASS | Conta vendedora de teste `TESTUSER4451475770762391424`, site `MLB`, pode vender (rodado pelo agente) |
| 2026-11-16T06:34Z | `490dee1` | `deploy-execucao` | PASS | OK smoke_deploy execucao: pipeline verde, 200 com token do Cadastros e 401 sem token |
| 2026-11-16T06:34Z | `490dee1` | `mercado-pago` | PASS | OK smoke_mercado_pago: credenciais no pod da Cobrança e webhook recusando assinatura inválida |
| 2026-11-16T06:40Z | `f335497` | Notificação simulada do Mercado Pago (manual, José) | FAIL | Assinatura aceita (passou da validação); consulta do pagamento inventado `123456` deu 404 no Mercado Pago e a Cobrança respondeu `500` em vez de `422`. Logs também mostraram `AWSSDK.SecurityToken` ausente nos 4 serviços (IRSA sem credencial; SQS/DynamoDB falhando) |
| 2026-11-16T07:11Z | `36cab07` | `aws-credenciais` | PASS | OK smoke_aws_credenciais: os, cobranca, execucao e cadastros sem erro de credencial da AWS em 10m |
| 2026-11-16T07:11Z | `36cab07` | `deploy-execucao` | PASS | OK smoke_deploy execucao: pipeline verde, 200 com token do Cadastros e 401 sem token |
| 2026-11-16T07:11Z | `36cab07` | `mercado-pago` | PASS | OK smoke_mercado_pago: credenciais no pod da Cobrança e webhook recusando assinatura inválida |
| 2026-11-16T07:12Z | `36cab07` | `aws-credenciais` | PASS | OK smoke_aws_credenciais: os, cobranca, execucao e cadastros sem erro de credencial da AWS em 10m |
| 2026-11-16T07:15Z | `9736cc5` | Notificação simulada do Mercado Pago (manual, José) | PASS | Painel: `payment.updated` id `123456` → `422 Unprocessable Entity` (o painel marca como falha por não ser 2xx). Log da Cobrança `9817ecf`: assinatura aceita, `GET /v1/payments/123456` → 404, sem exceção |
