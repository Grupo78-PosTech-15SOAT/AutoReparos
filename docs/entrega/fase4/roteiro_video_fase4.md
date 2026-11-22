# Roteiro do vídeo — Fase 4 (13–14 min, máx. 15)

> Base: [spec do vídeo](../../../.specs/features/fase4/especificacao/entregaveis/fase4_spec_video_e_documento_portal.md) e D-76 do [índice](../../../.specs/features/fase4/especificacao/index.md). Grave bloco a bloco e corte as esperas.

## Antes de gravar (15 min)

**Comandos (em todo terminal novo, comece pelo `cd`):**

```bash
cd ~/Code/AutoReparos
```

```bash
aws login --profile autoreparos
aws sts get-caller-identity --profile autoreparos   # conta 198903849859
aws eks update-kubeconfig --name AutoReparosEKS --region us-east-1 --profile autoreparos
kubectl get pods -A                                 # tudo Running
scripts/smoke/rodar.sh                              # smoke verde
```

```bash
# terminal 2 — Jaeger da AWS em http://localhost:16687 (a 16686 é do Jaeger local do docker compose)
kubectl port-forward -n autoreparos svc/autoreparos-jaeger 16687:16686
```

```bash
# terminal 3 — Grafana da AWS em http://localhost:3001/grafana/ (com /grafana/ no fim; usuário admin; a 3000 é do Grafana local)
kubectl port-forward -n autoreparos svc/autoreparos-grafana 3001:3000
```

```bash
# senha do Grafana
aws ssm get-parameter --profile autoreparos --region us-east-1 --with-decryption \
  --name /autoreparos/production/plataforma/grafana-admin-password --query Parameter.Value --output text
```

```bash
# bloco 5 — BDD (6 cenários)
dotnet test submodules/AutoReparos.OS/AutoReparos.OS.BddTests
```

```bash
# bloco 7 — pod novo da Cobrança
kubectl get pods -n cobranca -w
```

**Links:** Web `https://8afyzjqfk2.execute-api.us-east-1.amazonaws.com/` · Postman: `postman/` de OS, Cobrança e Execução

**Checklist:**

- [ ] Web abre e o login funciona
- [ ] Login do Grafana funciona (se não funcionar, a senha do SSM diverge do Secret: avise antes de gravar)
- [ ] **OS-B pronta:** OS com insumo, orçamento aprovado e reserva feita (aparece no drawer da Saga)
- [ ] Cliente com **telefone novo**; anotar CPF e e-mail
- [ ] Cobrança: [PR #20](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.Cobranca/pull/20) vermelho e bloqueado; run [`44199386475`](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.Cobranca/actions/runs/44199386475) da `main` parado em `deploy` (**não aprovar antes de gravar**)
- [ ] Abas: Web, aba anônima, Postman, GitHub, SonarCloud, Mercado Pago, Jaeger, Grafana, VS Code
- [ ] Zoom 125–150% e notificações desligadas

## Blocos

### 1 · Abertura — 00:00 (1 min)
**Tela:** [diagrama](diagrama_arquitetura.png)
**Fala:** Grupo 78, Fase 4: o sistema da oficina dividido em microsserviços com Saga orquestrada.

### 2 · Arquitetura — 01:00 (1,5 min)
**Tela:** [diagrama geral](diagrama_arquitetura.png), depois o [diagrama da Saga orquestrada](diagrama_saga_orquestrada.mmd) e a organização no GitHub
**Fala:** OS orquestra a Saga · Cobrança faz orçamento e pagamento · Execução cuida de fila e insumos · Cadastros guarda clientes e emite o login. Um PostgreSQL por serviço, a Saga no DynamoDB, mensagens por SQS e SNS, cada serviço com o próprio deploy no EKS.

### 3 · Código — 02:30 (1,5 min)
**Tela:** VS Code
- [`SagaOS.cs`](../../../submodules/AutoReparos.OS/AutoReparos.OS.Domain/SagaOS.cs) **linhas 91–98**: cada etapa com a sua compensação
- `SagaOS.cs` **linha 598**: compensa da última etapa para a primeira
- [`OrquestradorSaga.cs`](../../../submodules/AutoReparos.OS/AutoReparos.OS.Application/Saga/OrquestradorSaga.cs) **linha 47**: mensagem repetida é descartada pelo `EventId`

### 4 · Fluxo completo, OS-A — 04:00 (3,5 min)
**Tela:** Web
1. Nova OS (serviço + insumo)
2. Orçamento
3. Aba anônima → Aprovar orçamento (CPF + e-mail)
4. Kanban / Sagas: reserva e liberação pela Saga
5. Fila de execução: etapas → concluir
6. Pagamento → checkout do Mercado Pago (comprador de teste)
7. OS `Paga`, Saga `PagamentoCapturado`, histórico

**Copie o id da OS-A** para o bloco 6.

### 5 · Saga com falha, OS-B — 07:30 (2 min)
**Tela:** Web e terminal
1. Detalhe da OS-B → **Cancelar** (com motivo)
2. Sagas → drawer: `cancelar execução` → `cancelar reserva` → `cancelar orçamento` → `Compensada`
3. Saída do BDD: 6 cenários verdes

**Fala:** "A Saga desfaz o que já foi feito, do último passo para o primeiro."

### 6 · Monitoramento — 09:30 (2 min)
**Tela:** Jaeger e Grafana
1. Jaeger: tag `ordem_servico.id` = id da OS-A → trace Cobrança → OS → Execução
2. Grafana: dashboard **Saga** e um painel de serviço
3. Loki: logs da OS-A

### 7 · Deploy com testes — 11:30 (2,5 min)
**Tela:** GitHub, Actions, terminal
1. Settings → Branches da Cobrança: PR obrigatório, `build` e `test` obrigatórios
2. [PR #20](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.Cobranca/pull/20): bug no total do orçamento → `test` vermelho → **merge bloqueado**
3. [PR #18](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.Cobranca/pull/18) já mesclado: correção real (pod reinicia quando o segredo muda), checks verdes
4. Run [`44199386475`](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.Cobranca/actions/runs/44199386475): `build` → `test` → `quality` verdes, `deploy` esperando → **Review deployments → Approve** ao vivo
5. Terminal: `kubectl get pods -n cobranca -w` mostrando o pod novo subindo e o antigo saindo
6. SonarCloud da Cobrança com a cobertura

**Fala:** "Se o `/health` da versão nova falhar, o Helm volta sozinho para a anterior."

### 8 · Encerramento — 13:45 (30 s)
Fluxo da OS ✔ · Saga e falhas ✔ · Deploy com testes ✔ · Monitoramento ✔

## Se travar

| Problema | Faça |
|---|---|
| Passo do Web | Faça o mesmo passo pelo Postman e volte ao Web |
| Webhook não chega | Mercado Pago → "Simular notificação" com o id real |
| CI lento | Corte na edição |
| Grafana vazio | Janela de 30 min |

## Depois

Publicar como **não listado**, testar em aba anônima → link no [PDF](template_entrega_portal_fiap.md) → fechar o `demo/teste-quebrado` → `destroy-aws.yml`.
