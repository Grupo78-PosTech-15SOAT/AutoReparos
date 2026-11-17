# Adaptação do Web às APIs dos serviços verification

**Verdict**: PASS
**Mutations**: 3 - probed C4, C10, C15
**Proofs**: `proofs.md` at a500dde
**Verifier**: independent sub-agent (author != verifier)

## Findings

None - sondadas 3 mutações em código de produção (C4 em `jwt.interceptor.ts:10`, C10 em `orcamento-page.component.ts:153` e C15 em `sagas-page.component.ts:271`, 3/3 mortas), 2 exemplos próprios de fronteira em `OrcamentoPageComponent`, `OsKanbanPageComponent` e `SagaService` e auditoria das asserções de C1 a C20 contra `plan.md` e `checks.md`.

## Mutations injected

| Mutation | Location | Killed |
| --- | --- | --- |
| exigeToken prefixo /api/v1/ -> qualquer /api/ (vaza token no login e legado) | `submodules/AutoReparos.Web/src/app/core/http/jwt.interceptor.ts:10` | yes |
| calculo do total do orcamento ignora quantidade do item | `submodules/AutoReparos.Web/src/app/features/orcamento/pages/orcamento-page/orcamento-page.component.ts:153` | yes |
| retentativa de saga aceita envio com motivo em branco | `submodules/AutoReparos.Web/src/app/features/sagas/pages/sagas-page/sagas-page.component.ts:271` | yes |

## Own examples

1. **C10 (`OrcamentoPageComponent`) — quantidades não positivas, valores fracionados, remoção intermediária e IDs em branco:** em teste temporário executado em `HEAD` (removido após a execução), adicionar serviço de `33.33` com quantidade `3`, insumo de `0.01` com quantidade `-5` (normalizada para `1` por `Math.max(1, Number(quantidade) || 1)`) e insumo de `12.50` com quantidade `2` produziu total `125.00`; remover o item central recalculou o total para `124.99`; acionar `enviar()` com `ordemServicoId = '   '` bloqueou o `POST /api/v1/orcamentos`.
2. **C6 e C15 (`OsKanbanPageComponent.mapearStatusParaColuna` e `SagaService.listar`) — precedência de `intervencaoRequerida`, estados intermediários/desconhecidos e limite inferior de `paradaHaMinutos`:** em teste temporário em `HEAD`, `mapearStatusParaColuna('EmExecucao', true)` priorizou `'IntervencaoRequerida'` sobre `'EmExecucao'`, `'CompensacaoFalhou'` mapeou para `'IntervencaoRequerida'`, `'AguardandoInsumos'` para `'OrcamentoAprovado'`, `'ExecucaoLiberada'` para `'EmExecucao'` e um estado desconhecido caiu no fallback `'Recebida'`; `SagaService.listar({ estado: '', paradaHaMinutos: 0 })` omitiu query params, enquanto `paradaHaMinutos: 1440` enviou `paradaHaMinutos=1440`.

## Assertions audit

- **S1 (C1–C4):** `api-endpoints.spec.ts` exercita todos os 25 endpoints `/api/v1/...`, mais `AUTH.LOGIN` (`/api/auth/login`) e `AUTH.CLIENTE` (`/auth/cliente`), e confirma ausência de `DASHBOARD`, `ENTREGAR` e `STATUS`. `assert_web_adaptacao.py --check proxy` valida as 12 locations de proxy, fallback 404 em `/api/v1/`, `/health` 200, `/` SPA e substituição em `docker-entrypoint.sh`. `error.interceptor.spec.ts` testa o produto cartesiano dos 6 status (`401, 403, 404, 409, 422, 503`) com os códigos do catálogo, código desconhecido e corpo nulo. `jwt.interceptor.spec.ts` comprova envio restrito a `${API_BASE_URL}/api/v1/`, preservação de `Authorization` explícito do cliente e limpeza de sessão com redirect em 401 fora do login.
- **S2 (C5–C9):** `os-nova-page.component.spec.ts`, `os-kanban-page.component.spec.ts`, `os-detalhe-page.component.spec.ts`, `os-cancelamento.component.spec.ts` e `consulta-publica-page.component.spec.ts` verificam método HTTP, URL, payload e renderização DOM dos 7 status reais da Saga, `PATCH` de observação, conflitos 409 (`saga_em_andamento`, `cancelamento_indisponivel`) e ausência de busca por placa/CPF.
- **S3 (C10–C12):** `orcamento-page.component.spec.ts`, `aprovacao-orcamento-page.component.spec.ts` e `pagamento-page.component.spec.ts` provam soma do total no cliente, `POST /auth/cliente` seguido de `POST /api/v1/orcamentos/{id}/aprovacao` com `Bearer` do cliente, exibição de `pagamentoId`, tratamento de 401/403/404/409, `POST /api/v1/pagamentos/{id}/checkout` com `urlCheckout` e reembolso restrito a Administrador.
- **S4–S6 (C13–C20):** `fila-execucao-page.component.spec.ts`, `insumos-page.component.spec.ts`, `servicos-page.component.spec.ts`, `sagas-page.component.spec.ts`, `role.guard.spec.ts` e `assert_web_adaptacao.py` (`legado`, `cobertura`, `node`) provam os filtros, comandos de execução/catálogo/Sagas, ocultação de controles não suportados pelo backend, remoção completa dos 5 itens legados, cobertura acima de 80% nos arquivos novos/alterados e execução em Node 25/26 sem flags extras.
