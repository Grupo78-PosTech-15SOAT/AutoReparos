# Project state

## Decisions

| ID | Decision | Rationale | Status | Date |
| --- | --- | --- | --- | --- |
| AD-001 | Integrações transitórias usam limites padrão de retry por dependência, backoff exponencial com jitter e DLQ/alerta ao esgotar; cobrança capturada nunca é repetida | User: "Adote limites padrão por dependência, backoff exponencial com jitter e DLQ/alerta ao esgotar; nunca repetir cobrança capturada." Evita retries ilimitados e cobranças duplicadas | active | 2026-11-06 |
