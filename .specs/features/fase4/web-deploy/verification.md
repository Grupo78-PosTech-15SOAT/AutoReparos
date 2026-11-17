# Deploy próprio do Web verification

**Verdict**: FAIL
**Mutations**: 3 - probed C8, C1, C14 (2 killed, 1 not)
**Proofs**: `proofs.md` at 216a6d3 (Web `71d97d1` = conteúdo de `origin/main` `c3165d7` em `.github/`, `deploy/` e `sonar-project.properties`; Infra.K8s `71e67e0`) - 21 pass, 0 fail
**Verifier**: independent sub-agent (author != verifier)

## Findings

| Finding | Evidence | Fix | Closed |
| --- | --- | --- | --- |
| O pai pode religar o subchart `web` no release `autoreparos` sem nenhuma prova estática falhar (AC 10, porta 1): `--set web.enabled=true` no `helm upgrade` do `deploy-app.yml` passa no C14, e o C13 só lê o `values-production.yaml` | mutação `C14/AC 10, porta 1: deploy-app do pai religa o subchart web com --set web.enabled=true` não morta (`.github/workflows/deploy-app.yml:125`, prova `assert_web_deploy.py --check pai-sem-web`); `checar_pai_sem_web` só proíbe `web\.image`, `ECR_WEB`, `docker build/push` etc. | no `checar_pai_sem_web`, recusar qualquer `--set`/`--set-string` com `web.` no `helm upgrade` do pai (ou, melhor, renderizar o comando do pai com `helm template` e exigir que não saia nenhum objeto `autoreparos-web`) |  `assert_web_deploy.py` `pai-sem-web` recusa `--set`/`--set-string` com `web.` no `helm upgrade` do pai; mutação C14 morta |
| Com `always()`, cancelar um `workflow_dispatch` com `ignorar_quality: true` enquanto o `quality` roda não impede o deploy em produção: build e test `success`, quality `cancelled` e o input verdadeiro fazem o `deploy` rodar (o GitHub executa jobs com `always()` mesmo depois do cancelamento do run) | exemplo próprio no avaliador da prova C8 (`condicao_do_job` de `assert_web_deploy.py`): `dispatch main input, quality cancelled -> True`; condição em `submodules/AutoReparos.Web/.github/workflows/ci-cd.yml:96` começa com `always() &&`; a tabela de 10 casos do C8 não tem `cancelled` | trocar `always()` por `!cancelled()` na condição do `deploy` (mantém o escape do quality `failure`) e acrescentar ao `CASOS_DEPLOY` o caso "dispatch main com input e quality cancelled pula", com o avaliador tratando `cancelled()` pelo estado do run |  Web `628ef35` (PR #7): `!cancelled()` no lugar de `always()`; caso "run cancelado no quality" no `deploy-condicao`, que falhava antes |
| O C12 prova só metade do AC 9: lê o último run de `push` na `main`; o quality verde num PR para a `main` não tem prova | `checar_quality_verde` chama `ultimo_run_main({"push"})`; o comportamento hoje está correto (run `44021213127`, `pull_request` de `develop` para `main`, `quality success`), mas nada falharia se voltasse a ficar vermelho | estender `quality-verde` para exigir também o último run `pull_request` com base `main` com `quality` `success` |  `quality-verde` exige também o quality verde do último PR com base `main` (PR #6, run `44021213127`) |

## Probed sem achado

- Mutação C8 (`deploy` deixa de exigir `test` verde com `ignorar_quality`): morta por `deploy-condicao`.
- Mutação C1 (access entry da role do Web com edição também no namespace `autoreparos`): morta por `tf_test.sh ... web-deploy`.
- Exemplos próprios na condição do `deploy`: push na `main` com quality `skipped` pula; push de tag `refs/tags/v1` pula; dispatch na `main` com input e test `cancelled` pula; PR para a `main` todo verde pula.
- Exemplos próprios no simulador do IAM (fora dos 17 casos do C2): `ecr:PutImage` em `repository/autoreparos-web/sub` e no mesmo repositório em `us-west-2`, `eks:ListClusters`, `eks:UpdateClusterConfig` e `sts:AssumeRole` em outra role de deploy são negados. `ecr:DeleteRepository` e `ecr:SetRepositoryPolicy` em `autoreparos-web` são permitidos, como o `ecr:*` literal da porta 2 aprovada no plano prevê.
- AC 9 (causa do código 3): o job `quality` do run `43251757645` foi reexecutado em 17/11 e passou sem mudança de configuração, o que bate com o diagnóstico "primeira análise da branch sem linha de base" registrado no `ci-cd.yml`.
