# Validação independente — renomeação para `servico-os`

**Data:** 2026-11-11  
**Verificador:** `spec_rename_verifier` (`author != verifier`)  
**Decisão:** D-27 / AD-029  
**Veredito:** PASS para a renomeação estrutural. O gate TLC completo permanece pendente de uma árvore Git limpa e de provas no `HEAD` atual.

## Evidências

| Verificação | Resultado |
| --- | --- |
| `python scripts/specs/verificar_links.py` | 54 arquivos, 204 links, 0 problemas |
| `validate_plan.py fase4/servico-os` | 0 erros, 0 avisos |
| `validate_checks.py fase4/servico-os` | 0 erros, 0 avisos; 3 mutações declaradas |
| `bash -n` em `proof-env/run_proofs.sh` e `proof-env/harness.sh` | PASS |
| `git check-attr text eol` | Scripts `.sh` e `.py` do ambiente de prova com `text` e `eol=lf` |
| `git diff --check -- .gitattributes .specs` | PASS |
| Inventário da pasta `servico-os` | 9 arquivos originais presentes |
| Busca por `fase4/os-service` e `os-service/` fora das evidências históricas | Nenhuma referência operacional restante |

O `mutations.json` novo é semanticamente idêntico ao anterior em `HEAD:.specs/features/fase4/os-service/mutations.json`: os três alvos, alterações e comandos de prova foram preservados. `plan.md`, `proofs.md` e `verification.md` mantêm seu conteúdo histórico; as mudanças no `checks.md` limitam-se ao caminho do plano e ao registro deste Verifier. Os scripts do ambiente de prova mudaram apenas nos nomes da pasta/feature e em texto descritivo.

## Gate pendente

`validate_verification.py fase4/servico-os` retorna erro: `proofs.md` foi escrito em `ba95d75`, enquanto o `HEAD` atual é `2b04d16`. A árvore de trabalho contém alterações simultâneas fora desta renomeação. Por isso `check_proofs.py` e o sensor de três mutações não foram reexecutados: o gate exige árvore limpa e a prova C5 exige o ambiente local descrito em `proof-env/README.md`. Não alterei o `proofs.md` histórico nem fiz commit ou stash de trabalho alheio.

## Open items

- Reexecutar `check_proofs.py fase4/servico-os --mutations .specs/features/fase4/servico-os/mutations.json` no `HEAD` definitivo e com a árvore limpa, usando `proof-env/run_proofs.sh` para provisionar a prova C5; então rodar `validate_verification.py fase4/servico-os`.
