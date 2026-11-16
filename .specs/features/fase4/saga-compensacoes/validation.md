# Saga compensações validation

**Data:** 2026-11-13
**HEAD:** f86ee37 (árvore limpa; `proofs.md` e `mutations.json` ainda não commitados no início da verificação)
**Veredito:** FAIL

## Escopo

Verificação independente (author != verifier) de `fase4/saga-compensacoes` contra `plan.md` (critérios 1 a 12 e 6a) e `checks.md` (C1 a C14), por leitura do código novo do OS, da Cobrança e da Execução e dos testes que os checks nomeiam, mais quatro exemplos próprios e seis mutações. Detalhe e evidências em `verification.md`.

## Check por check

| Check | Julgamento |
| --- | --- |
| C1, C2, C3, C4, C6, C7, C13, C14 | Código e teste batem com o critério; C4 e C14 provados por mutação (mortas). O teste de C14 usa `eventId` novo na segunda tentativa, por isso não vê o achado do `eventId` consumido |
| C5, C9 | Reenvio, esgotamento e compensação provados (mutação do limite morta). Prazo da reserva x prazo da liberação não distinguível pelos testes (mutação sobrevivente); a liberação só tem prazo enquanto não entregue |
| C8 | Teste correto em `AutoReparos.OS.Tests`, mas a prova de `checks.md` aponta `IntegrationTests` e passa vazia |
| C10 | Mutação do intervalo morta; cobre 2 de 3 prazos de negócio do AC 10 |
| C11 | 401/403/404/409/422 e histórico com autor, motivo e data provados |
| C12 | 403 e 422 provadas; 401 não provado nos três serviços; status 200 em vez de 202; roteamento da DLQ do OS só testado em 4 tipos (mutação sobrevivente) |

## Mutações

6 injetadas (3 pedidas e 3 extras): mortas C9 (reenvios), C10 (lembrete), C14 (trava D-33) e C4 (ordem inversa); sobreviventes C5/C9 (prazo da reserva) e C12 (roteamento de `reservar-insumos`). Spec em `mutations.json`; árvore restaurada byte a byte.

## Falhas reais encontradas

Lembrete bloqueando compensação (reproduzido), `eventId` de cancelamento consumido por rejeição (reproduzido), soma por insumo acima do limite na Cobrança (lido). Sem achado: documento de Saga antigo (V3 verde), reenvio duplicado de compensação, escrita condicional por versão.

## Open items

Reserva órfã pela ordem do SQS padrão; execução real em AWS, consumidor do Cadastros, `dotnet format` e README não verificados.
