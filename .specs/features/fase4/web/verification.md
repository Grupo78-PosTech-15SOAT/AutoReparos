# Extração da aplicação Web verification

**Verdict**: PASS
**Mutations**: 5 - C7 (push), C7 (PR), C5 (fallback), C5 (HTML genérico) e C6 (serialização); 5/5 mortas.
**Proofs**: `proofs.md` em `cf31a63` (12/12 provas, 5/5 mutações)
**Verifier**: subagente independente (author != verifier)

## Findings

| Finding | Evidence | Fix | Closed |
| --- | --- | --- | --- |
| F1 - C6: `API_URL` com apóstrofo gerava JavaScript inválido. | Na versão anterior, `docker-entrypoint.sh` interpolava o valor entre aspas simples. Na imagem de Web `028d4da`, o exemplo independente `https://api.example.test/cliente/D'Avila` foi avaliado por Node sem erro e `window.__env.apiUrl` manteve o valor exato. A prova C6 agora executa `env.js` e compara o resultado; a mutação que remove a serialização foi morta. | Serialização dos bytes da URL e `decodeURIComponent` no `env.js`, com prova de igualdade após execução. | Web `028d4da`; prova pai `f63ae56`; mutação final `cf31a63` |
| F2 - C7: a prova não vinculava branches aos gatilhos. | A prova agora lê as branches sob cada evento. Em workflow temporário com apenas `pull_request.branches: [develop]`, `web_ci()` falhou com `PR do Web não aponta só para main`; o workflow real mantém PR em `main` e push em `develop`/`main`. A mutação de PR errado foi morta. | Asserções específicas para `pull_request` e `push` e mutação da branch de PR. | Pai `f63ae56`; seletor da mutação `cf31a63` |
| F3 - C5: o smoke aceitava HTML genérico 200. | A prova compara as respostas de `/` e `/login` com `/usr/share/nginx/html/index.html` e exige bundle Angular presente. A imagem Web `028d4da` serviu o índice exato nas duas rotas. Um Nginx temporário que devolvia `<html>placeholder</html>` com HTTP 200 nas duas rotas divergiu do índice e seria rejeitado pela prova; a mutação correspondente foi morta. | Igualdade com o índice gerado e presença do bundle Angular, além de mutação HTML genérico 200. | Pai `f63ae56`; mutação final `cf31a63` |

## Exemplos independentes

1. **C6:** `API_URL=https://api.example.test/cliente/D'Avila` na imagem reconstruída de Web `028d4da` resultou em JavaScript válido e valor idêntico no navegador simulado.
2. **C7:** alterar somente o gatilho de PR para `[develop]` em workflow temporário foi rejeitado.
3. **C5:** `/` e `/login` serviram o índice Angular exato; um Nginx temporário com HTML genérico 200 nas duas rotas foi rejeitado pela comparação de conteúdo.

## Escopo externo

Configuração do projeto e secret SonarCloud, proteção de `main` e andamento do PR #8 dependem de sistemas externos. Este PASS verifica a extração e as provas locais; não declara essas pendências concluídas.
