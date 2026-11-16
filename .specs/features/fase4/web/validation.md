# Validação independente — extração da aplicação Web

**Data:** 2026-11-12
**Verificador:** `web_verifier` (`author != verifier`)
**HEAD pai:** `cf31a63`
**Submódulos:** Web `028d4da`; Cadastros `961b056`
**Veredito:** PASS — F1, F2 e F3 fechados por commits e provas reavaliadas.

## Evidências

| Verificação | Resultado |
| --- | --- |
| `proofs.md` em `cf31a63` | 12/12 provas pass; builds Angular e .NET pass. |
| `mutations.json` | 5/5 mortas: push `develop`, PR `main`, fallback de `/login`, HTML genérico 200 e serialização de URL com apóstrofo. |
| Diff pai `38fab61..cf31a63` e Web `5d1aeae..028d4da` | C6 corrigido no Web; provas C5-C7 fortalecidas no pai; mutação final ajustada em `cf31a63`. |
| URL independente com apóstrofo | `https://api.example.test/cliente/D'Avila` passou pela imagem reconstruída, `env.js` executou no Node e expôs o mesmo valor. |
| Gatilho de PR incorreto em diretório temporário | `pull_request.branches: [develop]` foi rejeitado por `web_ci()`; nenhuma alteração na árvore do projeto. |
| Nginx temporário com HTML genérico 200 | `/` e `/login` responderam 200 com `<html>placeholder</html>`; o corpo diferiu do índice Angular e a nova prova C5 o rejeita. |
| Imagem Web reconstruída | `/` e `/login` responderam 200 com corpo idêntico ao `index.html` da imagem. |

## Gate TLC

`validate_verification.py fase4/web`: PASS após fechamento dos três achados em `verification.md`. Os commits Web `028d4da`, pai `f63ae56` e `cf31a63` identificam as correções e mutações.

## Pendências externas

Cadastro do projeto e do secret SonarCloud, proteção de `main` e andamento do PR #8 não foram verificados nesta rodada local. Não são contabilizados como falhas da extração nem declarados concluídos.
