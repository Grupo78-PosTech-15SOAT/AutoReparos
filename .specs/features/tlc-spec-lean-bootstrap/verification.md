# TLC Spec Lean bootstrap verification

**Verdict**: FAIL
**Mutations**: 3 - propostas C3, C2 e C2; não executadas
**Proofs**: `validate_project_config.py`, `validate_plan.py` e `validate_checks.py` passaram em Python 3.14.0; `check_proofs.py` não foi executado
**Verifier**: independent sub-agent (author != verifier)

## Findings

| Finding | Evidence | Fix | Closed |
| --- | --- | --- | --- |
| O inventário exigido pelo validador ainda omite dependências do pacote | `validate_project_config.py` agora exige os gates principais, mas não exige `scripts/lean_config.py` (importado pelos validadores), `scripts/runners.py` (importado por `check_proofs.py`), `scripts/fixtures/` (usado por `selftest.py`) nem as referências ligadas por `SKILL.md`. Remover esses arquivos ainda pode deixar C2 verde e quebrar o fluxo. | Completar o inventário de dependências e referências, ou adicionar uma verificação automatizada de integridade dos links e imports do pacote. | |

## Resolved findings from earlier rounds

- C3 chama `validate_checks.py`, que verifica a aprovação explícita.
- C2 compara a configuração `mutations: 3` com o valor de `checks.md` e agora requer os principais scripts de fase.
- C4 compara com `HEAD`, incluindo alterações staged.
- `.specs/LESSONS.md` aponta para `.specs/tlc-spec-lean/scripts/lessons.py`.
- O plano identifica `.agents/AGENTS.md` como instrução TLC e `AGENTS.md` como ponte.
- A seção TLC duplicada em `.agents/AGENTS.md` foi removida.

## Gate results and limitations

`validate_project_config.py` passou: `TLC config valid for tlc-spec-lean-bootstrap: mutations=3`. `validate_plan.py` passou com 0 erros e 3 avisos sobre nomes de nós do diagrama; `validate_checks.py` passou com 0 erros e 3 avisos sobre ausência de seletores nos proofs de comando. Não rodei `check_proofs.py` nem mutações, conforme solicitado. O estado da árvore continua dirty fora de `.specs/` e os submódulos também estão alterados, então o gate completo precisa aguardar uma árvore válida para essa execução.
