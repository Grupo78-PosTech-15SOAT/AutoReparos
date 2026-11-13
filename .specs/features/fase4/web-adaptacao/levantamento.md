# Adaptação do Web - levantamento (12/11/2026)

Insumo para o `plan.md` da feature, ainda não escrito. Pesquisa somente leitura feita no Web `028d4da`, Cadastros `961b056`, OS `e1e2eef`, Cobrança `ab61119`, Execução `fc24261` e Infra.K8s `f6f9789`.

## Resumo

- O Web chama só o monólito Cadastros em `/api/...`, sem `v1`. A URL base é única (`window.__env.apiUrl`, gerado de `API_URL`).
- Parte das chamadas atuais já não batia com o próprio Cadastros antes da divisão:
  - o Web usa POST/PUT onde o backend espera PATCH (`iniciar-diagnostico`, `enviar-para-aprovacao`, `servicos/{id}/concluir`, `entregar`);
  - a aprovação do orçamento é GET com `?token`;
  - `PATCH /status` não existe;
  - usuários usam `PUT /{id}`, não `PATCH /role`.
- OS, Cobrança e Execução expõem poucas rotas, todas sob `/api/v1/...`, e nenhuma tem CORS. Faltam rotas de leitura: lista, Kanban, consulta pública e dashboard (OS); GET e recusa de orçamento e GET de pagamento (Cobrança); console `admin/sagas` (só no plano `saga-compensacoes`). Catálogo e insumos na Execução existem só no plano `insumos`.
- O D-21 (JWT RS256 + JWKS) não foi implementado. O Cadastros emite JWT simétrico, sem issuer nem audience, e os serviços novos não o validam. O token da AuthLambda não traz a claim `clienteId` que a aprovação na Cobrança exige.
- Roteamento:
  - Ingress e API Gateway mandam todo o `/api` para o Cadastros. Não existem charts nem Ingress dos serviços novos.
  - O compose local sobe só o Cadastros e o Web, sem gateway local.
  - A spec (Separação § 5, Etapa 5) pede uma URL base, com roteamento por path no API Gateway e no Ingress.
  - Conflito: `/api/v1/admin/dlq/reprocessar` tem o mesmo path em três serviços.
- Testes: há um único spec (`src/app/app.spec.ts`, 2 testes) com vitest e jsdom. No Node 25+ ele falha por causa do `localStorage` nativo do Node. O usuário decidiu corrigir isso nesta feature.

## Rotas Angular atuais (`src/app/app.routes.ts`)

- **Públicas:** `''`, `login`, `consulta-publica`, `aprovar-orcamento?token=`.
- **Com autenticação:**
  - `ordens-servico/fila` (Kanban)
  - `ordens-servico/nova` (Atendente, Administrador)
  - `ordens-servico/:id`
  - `ordens-servico`
  - `clientes`
  - `veiculos`
  - `insumos` (Administrador, Mecanico)
  - `servicos`
  - `usuarios` (Administrador)
  - `dashboard` (Administrador)
- **Telas que não existem:** orçamento com itens, pagamento, fila de execução, histórico, console de Sagas, login do cliente por CPF.

## Mapeamento legado → novo

| Legado do Web | Substituto |
|---|---|
| POST `/api/auth/login` | Cadastros, com path alvo indefinido. Cliente: AuthLambda `POST /auth/cliente` (sem tela hoje) |
| GET `/api/ordem-servico`, `/kanban`, `/consulta`, `/api/dashboard/metrics` | sem equivalente (a spec atribui ao OS) |
| POST `/api/ordem-servico` | OS `POST /api/v1/ordens-servico` com `{clienteId, veiculoId, observacao?}` |
| GET `/{id}` | OS `GET /api/v1/ordens-servico/{id}`: id, clienteId, veiculoId, observacao, status e histórico, sem itens |
| PATCH `/{id}/status` | removido; o status é dirigido pela Saga |
| `iniciar-diagnostico`, `servicos/{item}/concluir` | Execução `POST /api/v1/execucoes/{execId}/etapas` (`Diagnostico`/`Reparo`) e `/conclusao` |
| POST `/{id}/servicos`, `/{id}/insumos` | itens do orçamento na Cobrança, ainda não implementados |
| `enviar-para-aprovacao` | Cobrança `POST /api/v1/orcamentos` `{ordemServicoId, clienteId, valor}` |
| aprovação por token de e-mail | Cobrança `POST /api/v1/orcamentos/{id}/aprovacao`, exige JWT de Cliente com `clienteId`; não há recusa REST |
| `entregar` | sem equivalente |
| (novo) | OS `POST /{id}/cancelamento` (Administrador, Atendente); `PATCH /{id}` (observação); Cobrança `POST /pagamentos/{id}/checkout` e `/reembolsos`; Execução `GET /fila-execucao`, `/falha`, `/reexecucao` |
| CRUD clientes, veículos, usuários | Cadastros `/api/v1/...` pelo plano `cadastros`, ainda não migrado |
| CRUD serviços e insumos | Execução `/api/v1/servicos`, `/api/v1/insumos` pelo plano `insumos`, ainda não no código e sem PUT/DELETE |

## Decisões em aberto para o usuário

1. **URL base:** uma só, com proxy por path no nginx do Web ou num gateway no compose, ou uma URL por serviço? E quem cria Ingress e compose dos serviços novos?
2. **Rotas de leitura ausentes:** esta feature cria lista, Kanban, consulta, dashboard, GET de orçamento e pagamento e console, depende de outras features, ou as telas usam só o que existe?
3. **Login do operador e `/api/v1` do Cadastros:** qual path vale e se a migração acontece antes ou junto.
4. **JWT:** aceitar ambiente de demonstração com chave compartilhada ou esperar o JWKS (D-21)? O token da AuthLambda precisa ganhar a claim `clienteId`.
5. **Portal do cliente:** criar login por CPF e as telas "Minhas OS", aprovar e pagar? O fluxo por token de e-mail sai?
6. **Telas legadas sem equivalente** (dashboard, entregar, status manual, itens da OS): remover, esconder ou manter apontando para o Cadastros?
7. **Mínimo de telas novas para o vídeo de 16/11** (cronograma § 7: paths, orçamento e pagamento em 13/11; fila e console em 14/11).
8. **Contrato único de status da OS:** o código usa `"Recebida"`, a spec da Saga lista outros nomes e o Web tem `StatusOS` próprio.
9. **Testes do Web:** meta de cobertura (os 80% valem para o Web?) e a forma da correção para Node 25+ (`--no-experimental-webstorage` ou `STORAGE_TOKEN` em memória).
10. **Erros 409 e 422** com `{codigo}`: o `errorInterceptor` passa a mostrar mensagens por código?
