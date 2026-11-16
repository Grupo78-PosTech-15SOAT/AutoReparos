# Smoke tests do ambiente de produção

Testes que rodam contra o ambiente real (conta `198903849859`, `us-east-1`) depois de um deploy. Também são as provas dos checks do S6 de [`deploy-servicos`](../../.specs/features/fase4/deploy-servicos/checks.md); o plano os lista em [`plan.md` § Smoke tests](../../.specs/features/fase4/deploy-servicos/plan.md#smoke-tests).

Rode pelo `rodar.sh`, que registra cada execução em [`smoke.md`](../../.specs/features/fase4/deploy-servicos/smoke.md):

```bash
scripts/smoke/rodar.sh                     # todos
scripts/smoke/rodar.sh mercado-pago        # só um
scripts/smoke/rodar.sh aws-credenciais     # depois de cada deploy
```

| Script | Check | O que prova | Efeito no ambiente |
| --- | --- | --- | --- |
| `deploy-execucao.sh execucao` | C33 | Último pipeline da `main` da Execução com `deploy` verde; login no Cadastros pelo gateway; `GET /api/v1/servicos` com token → `200` e 5 serviços; sem token → `401` | Só leitura |
| `rollback-execucao.sh execucao` | C34 | Uma versão com `/health` quebrado fica `failed` e o `--atomic` volta à anterior; a API continua respondendo | Troca o Secret da Execução por alguns minutos e restaura (trap); acrescenta 2 revisões ao Helm |
| `mercado-pago.sh` | C41 | Pod da Cobrança recebe `MercadoPago__AccessToken` e `MercadoPago__WebhookSecret` de `cobranca-segredos`; webhook sem assinatura ou com assinatura falsa → `401` | Só leitura |
| `aws-credenciais.sh` | — | Logs dos pods de `os`, `cobranca`, `execucao` e `cadastros` nos últimos 10 minutos sem `AWSSDK.SecurityToken` (pacote do STS ausente, IRSA quebrado), `AccessDenied` nem `not authorized` | Só leitura |

Requisitos: `kubectl` com acesso ao cluster (`aws eks update-kubeconfig --name AutoReparosEKS --profile autoreparos`), `helm`, `gh` autenticado, `aws` com o perfil `autoreparos`, `curl` e `python3`.

## Testes manuais

Não dá para automatizar sem o painel ou um comprador de teste; registre o resultado à mão em [`smoke.md`](../../.specs/features/fase4/deploy-servicos/smoke.md).

| Teste | Passos | Esperado |
| --- | --- | --- |
| Notificação simulada do Mercado Pago | Painel do Mercado Pago → aplicação `AutoReparos Cobranca` → Webhooks → **Simular notificação** (evento Pagamentos) | A Cobrança aceita a assinatura: `422 pagamento_provedor_invalido` com id inventado (um `401` indica assinatura secreta errada); conferir com `kubectl logs -n cobranca deploy/cobranca` |
| Checkout de ponta a ponta | Login no Cadastros → abrir OS → orçamento na Cobrança → aprovar → abrir o `init_point` e pagar com um **comprador de teste** (outro usuário de teste, não o vendedor `TESTUSER4451475770762391424`) e cartão de teste | Webhook real recebido, pagamento aprovado e a Saga avança a OS |
