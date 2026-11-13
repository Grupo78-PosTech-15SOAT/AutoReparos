# Verificação independente — SonarCloud no plano Free

**Data:** 2026-11-12
**Veredito:** PASS para a condição de execução do `quality` nos cinco workflows; execução remota no GitHub ainda não observada.

## Escopo

Inspecionados `ci-cd.yml` de `AutoReparos.OS`, `AutoReparos.Web`, `AutoReparos.Cobranca`, `AutoReparos.Execucao` e `AutoReparos.Cadastros`. O `quality` depende de `test` e roda somente em `push` para `main` ou `pull_request` com base `main`. Os gatilhos de `push` em `develop` e `main` permanecem; `build` e `test` não ganharam filtro.

## Provas

- `scripts/specs/assert_pipelines.py --check jobs`: PASS, 5 repositórios.
- Expressão `if` extraída dos cinco workflows e avaliada em quatro cenários: push `main` = true; push `develop` = false; PR para `main` = true; PR para `develop` = false. PASS em todos.
- Sensor de discriminação: três mutações por workflow (trocar a branch de push, trocar a base do PR e trocar `||` por `&&`); todas alteraram o resultado esperado.
- Sintaxe conferida com [Workflow syntax](https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax) e [Contexts reference](https://docs.github.com/en/actions/reference/workflows-and-actions/contexts): `jobs.<job_id>.if` aceita expressão sem `${{ }}` e `github.base_ref` é a branch alvo do PR.
- [GitHub Docs](https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/control-jobs-with-conditions) confirma que job ignorado reporta sucesso mesmo sendo status check obrigatório; a proteção da `main` segue recebendo `quality` em PRs destinados à `main`.

## Open items

- `assert_pipelines.py --check qualidade` falha no Web por exigir a string `sonarcloud.io`, embora o workflow use `SonarSource/sonarqube-scan-action`; é limitação preexistente desse verificador, não da nova condição.
- Não houve execução remota dos cinco workflows neste ambiente; a prova cobre configuração e avaliação da condição, não autenticação do token nem resultado do quality gate no SonarCloud.
