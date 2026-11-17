# Validação independente — Adaptação do Web às APIs dos serviços (`fase4/web-adaptacao`)

**Data:** 2026-11-16
**Verificador:** Subagente independente `Verifier` (`author != verifier`)
**HEAD pai:** `a500dde`
**Base Web:** `6975b7f` → `ae5e108` (`submodules/AutoReparos.Web`, branch `feature/web-adaptacao`)
**Veredito:** PASS (0 achados abertos; 22/22 provas passando; 3/3 mutações mortas)

## Resumo dos Gates e Provas

| Gate / Verificação | Resultado | Evidência |
| --- | --- | --- |
| Provas de `checks.md` (C1 a C20 + Build) | PASS (22/22) | `proofs.md` em `a500dde` com todas as 20 verificações (22 linhas de prova + Build) verdes |
| Sensor de discriminação (`Mutations: 3`) | PASS (3/3 mortas) | `mutate.py --root . --spec .specs/features/fase4/web-adaptacao/mutations.json` → `3 killed, 0 survived, 0 blocked` |
| Exemplos próprios do Verificador (2 sondagens) | PASS (2/2) | Sondagens de fronteira em `OrcamentoPageComponent`, `OsKanbanPageComponent` e `SagaService` executadas via Vitest e removidas após confirmação |
| Auditoria de asserções (AC 1 a AC 20) | PASS | Asserções conferidas nos 13 `.spec.ts` do Web e nos 4 checks de `scripts/specs/assert_web_adaptacao.py` (`proxy`, `legado`, `cobertura`, `node`) |

## Mutações Injetadas (`mutations.json`)

| # | Check / Alvo | Arquivo | Mutação | Resultado |
| --- | --- | --- | --- | --- |
| 1 | C4 (`jwtInterceptor`) | `submodules/AutoReparos.Web/src/app/core/http/jwt.interceptor.ts:10` | `url.startsWith(PREFIXO_SURFACE)` → `url.includes('/api/')` | Morta (`yes`) |
| 2 | C10 (`OrcamentoPageComponent`) | `submodules/AutoReparos.Web/src/app/features/orcamento/pages/orcamento-page/orcamento-page.component.ts:153` | Soma do total ignora `item.quantidade` | Morta (`yes`) |
| 3 | C15 (`SagasPageComponent`) | `submodules/AutoReparos.Web/src/app/features/sagas/pages/sagas-page/sagas-page.component.ts:271` | Remove guarda `!motivo` em `retentar()` | Morta (`yes`) |

## Integridade da Árvore

`mutate.py` restaurou byte a byte todos os arquivos de código de produção de `submodules/AutoReparos.Web` após a execução das mutações, e o arquivo temporário de sondagem foi removido. Foram gravados apenas `.specs/features/fase4/web-adaptacao/mutations.json`, `.specs/features/fase4/web-adaptacao/verification.md` e `.specs/features/fase4/web-adaptacao/validation.md`.
