# Adaptação do Web às APIs dos serviços - checks

Mutations: 3
Build: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web build`
Plan: `.specs/features/fase4/web-adaptacao/plan.md`

22 checks in 6 slices · 1 one-way door · 0 open, of which 0 block

## Checks

### S1 - Camada HTTP e roteamento

**C1** - Todo endpoint de `api-endpoints.ts` começa em `API_URL` + `/api/v1` (ou `/api/auth/login` e `/auth/cliente`) e nenhum aponta para URL fixa de serviço ou rota removida (AC 1)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/config/api-endpoints.spec.ts --filter "ApiEndpoints"`

**C2** - O `nginx.conf` e o `docker-entrypoint.sh` do Web configuram o encaminhamento dos 4 grupos de prefixos `/api/v1/...` e `/api/auth` para `OS_URL`, `COBRANCA_URL`, `EXECUCAO_URL` e `CADASTROS_URL`, devolvem 404 para `/api/v1/` desconhecido e preservam `/` e `/health` com 200 (AC 2)
Proof: `python3 scripts/specs/assert_web_adaptacao.py --check proxy`

**C3** - O `errorInterceptor` mostra a mensagem do catálogo para cada `{codigo}` conhecido nos status 401, 403, 404, 409, 422 e 503, e a mensagem genérica por status para código desconhecido (AC 3)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/http/error.interceptor.spec.ts --filter "ErrorInterceptor"`

**C4** - O `jwtInterceptor` envia o token só às rotas `/api/v1/` autenticadas da Surface (excluindo `/api/auth/login`, `/auth/cliente` e origens externas), o login usa `POST /api/auth/login` e um 401 fora do login limpa a sessão e leva a `/login` (AC 4)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/http/jwt.interceptor.spec.ts --filter "JwtInterceptor"`

### S2 - Ordens de serviço e Kanban

**C5** - "Nova OS" chama `POST /api/v1/ordens-servico` com cliente e veículo, abre o detalhe da OS criada e mostra o erro 422 no formulário (AC 5)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-nova-page/os-nova-page.component.spec.ts --filter "OsNovaPage"`

**C6** - O Kanban mostra uma coluna para cada um dos 7 status reais da OS (`Recebida`, `OrcamentoAprovado`, `EmExecucao`, `Concluida`, `Paga`, `IntervencaoRequerida`, `Cancelada`), com estado vazio por coluna, indicador de carregamento e busca rápida por código da OS (AC 6)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-kanban-page/os-kanban-page.component.spec.ts --filter "OsKanbanPage"`

**C7** - O detalhe da OS mostra status, observação e histórico (`GET /api/v1/ordens-servico/{id}`), salva observação (`PATCH /api/v1/ordens-servico/{id}`), mostra o estado da execução e permite abrir orçamento, pagamento e cancelamento (AC 7)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-detalhe-page/os-detalhe-page.component.spec.ts --filter "OsDetalhePage"`

**C8** - "Cancelar OS" pede motivo, chama `POST /api/v1/ordens-servico/{id}/cancelamento`, mostra o estado devolvido e, em 409 `saga_em_andamento` ou `cancelamento_indisponivel`, mostra a mensagem do código e mantém a OS (AC 8)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/components/os-cancelamento/os-cancelamento.component.spec.ts --filter "OsCancelamento"`

**C9** - A consulta de OS busca pelo código (ID) da OS em `GET /api/v1/ordens-servico/{id}`, mostra status e histórico e não oferece busca por placa nem CPF (AC 9)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/portal-publico/pages/consulta-publica-page/consulta-publica-page.component.spec.ts --filter "ConsultaPublicaPage"`

### S3 - Orçamento, aprovação do cliente e pagamento

**C10** - A tela de orçamento lista serviços e insumos do catálogo da Execução com preço e saldo, soma o total e envia `POST /api/v1/orcamentos` com os itens, mostrando o valor, o status e o `id` devolvidos pela Cobrança (AC 10)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orcamento/pages/orcamento-page/orcamento-page.component.spec.ts --filter "OrcamentoPage"`

**C11** - A página de aprovação do orçamento autentica o cliente em `POST /auth/cliente` (CPF e e-mail), envia `POST /api/v1/orcamentos/{id}/aprovacao` com o token recebido, exibe a confirmação com o `pagamentoId` e mostra mensagem de erro em 401, 403, 404 ou 409 (AC 11)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/portal-publico/pages/aprovacao-orcamento-page/aprovacao-orcamento-page.component.spec.ts --filter "AprovacaoOrcamentoPage"`

**C12** - A tela de pagamento chama `POST /api/v1/pagamentos/{id}/checkout` ao acionar "Iniciar checkout", exibe/abre a `urlCheckout` devolvida e permite ao Administrador solicitar reembolso (`POST /api/v1/pagamentos/{id}/reembolsos`) com valor e referência (AC 12)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/pagamento/pages/pagamento-page/pagamento-page.component.spec.ts --filter "PagamentoPage"`

### S4 - Execução e catálogo

**C13** - A fila de execução filtra por status e mecânico em `GET /api/v1/fila-execucao` e executa registrar etapa (`Diagnostico` e `Reparo`), concluir, marcar falha e reexecutar, mostrando a mensagem de `execucao_estado_invalido` em 409 (AC 13)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/execucao/pages/fila-execucao-page/fila-execucao-page.component.spec.ts --filter "FilaExecucaoPage"`

**C14** - As telas de serviços e insumos usam só `GET/POST /api/v1/servicos`, `GET/POST /api/v1/insumos` e `POST /api/v1/insumos/{id}/entradas`, mostram saldo disponível e reservado e não oferecem editar nem excluir (AC 14)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/insumos/pages/insumos-page/insumos-page.component.spec.ts --filter "InsumosPage"`
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/servicos/pages/servicos-page/servicos-page.component.spec.ts --filter "ServicosPage"`

### S5 - Console de Sagas

**C15** - O console lista Sagas em `GET /api/v1/admin/sagas` com filtro de estado e de parada, abre o detalhe (`GET /api/v1/admin/sagas/{id}`) com etapas, comandos e registros e, em `AguardandoInsumos`, "Retentar reserva" exige motivo e chama `POST /api/v1/admin/sagas/{id}/retentar` (AC 15)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/sagas/pages/sagas-page/sagas-page.component.spec.ts --filter "SagasPage lista e retenta"`

**C16** - A retentativa com 409 `saga_em_andamento` ou `retentativa_indisponivel` mostra a mensagem do código e mantém a Saga como está (AC 16)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/sagas/pages/sagas-page/sagas-page.component.spec.ts --filter "SagasPage conflito"`

**C17** - O `roleGuard` e a navegação bloqueiam a rota `/sagas` e ocultam o item de menu para quem não é Administrador (AC 17)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/auth/role.guard.spec.ts --filter "RoleGuard"`

### S6 - Limpeza e qualidade

**C18** - O Web não contém rota, menu, serviço nem chamada para os 5 itens removidos: dashboard, entregar, status manual, itens soltos da OS e busca por placa/CPF (AC 18)
Proof: `python3 scripts/specs/assert_web_adaptacao.py --check legado`

**C19** - Os testes do Web passam com pelo menos 80% de cobertura de linhas nos arquivos novos ou alterados e `yarn build` sai com 0 (AC 19)
Proof: `python3 scripts/specs/assert_web_adaptacao.py --check cobertura`

**C20** - A suíte do Web passa em Node 25 ou 26 sem flag extra, com `src/test-setup.ts` configurado no `angular.json` (AC 20)
Proof: `python3 scripts/specs/assert_web_adaptacao.py --check node`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| prefixos de roteamento no Nginx (5) | `/api/v1/ordens-servico`+`/api/v1/admin/sagas` C2 · `/api/v1/orcamentos`+`/api/v1/pagamentos` C2 · `/api/v1/execucoes`+`/api/v1/fila-execucao`+`/api/v1/servicos`+`/api/v1/insumos` C2 · `/api/v1/clientes`+`/api/v1/veiculos`+`/api/v1/usuarios`+`/api/auth` C2 · `/api/v1/` desconhecido 404 C2 | - |
| status da OS no Kanban (7) | `Recebida` C6 · `OrcamentoAprovado` C6 · `EmExecucao` C6 · `Concluida` C6 · `Paga` C6 · `IntervencaoRequerida` C6 · `Cancelada` C6 | - |
| status HTTP tratados pelo interceptor (6) | 401 C3 · 403 C3 · 404 C3 · 409 C3 · 422 C3 · 503 C3 | - |
| códigos de erro do catálogo (8) | `saga_em_andamento` C3 · `orcamento_nao_pendente` C3 · `token_invalido` C3 · `filtro_invalido` C3 · `execucao_estado_invalido` C3 · `cancelamento_indisponivel` C3 · `retentativa_indisponivel` C3 · `dependencia_indisponivel` C3 | - |
| itens removidos do legado (5) | dashboard C18 · entregar C18 · status manual C18 · itens soltos C18 · busca por placa/CPF C18 | - |
| controles das telas novas/adaptadas (15) | Nova OS C5 · coluna do Kanban C6 · Salvar observação C7 · Cancelar OS C8 · Buscar por código C9 · Enviar orçamento C10 · Aprovar orçamento (CPF+e-mail) C11 · Iniciar checkout C12 · Reembolsar C12 · Registrar etapa C13 · Concluir C13 · Falha C13 · Reexecutar C13 · Registrar entrada C14 · Retentar reserva C15 | - |
| door 1: contrato de URL do navegador e proxy (2) | `api-endpoints.ts` C1 · proxy Nginx C2 | - |

## Swept

- validation: C5 (campos de abertura da OS), C10 (itens do orçamento), C11 (CPF/e-mail e código do orçamento), C14 (entrada de insumo), C15 (motivo da retentativa)
- failure modes: C3 (503 e códigos), C8 (conflito no cancelamento), C11 (falha na aprovação), C13 (conflito na execução), C16 (conflito na retentativa)
- idempotency: existing - os botões de envio desabilitam submissão duplicada enquanto `enviando()` está ativo (C8, C10, C11, C12, C13, C15)
- authorization: C4 (envio do JWT e logout em 401), C12 (reembolso restrito a Administrador), C13 (reexecução restrita a Administrador), C14 (criação no catálogo restrita a Administrador), C17 (`roleGuard` no console de Sagas)
- concurrency: C8, C13, C16 (tratamento de 409 devolvido pelos serviços quando há operação concorrente ou estado inválido)
- data lifecycle: C4 (limpeza de `autoreparos_token` e `autoreparos_user` em 401)
- dependency failure: C3 (tratamento de 503 `dependencia_indisponivel`)
- state transitions: C6 (7 status da Saga no Kanban), C13 (transições de execução), C15 (retentativa habilitada somente em `AguardandoInsumos`)
- observability: existing - `telemetry.init.ts` e proxy `/v1/traces` no `nginx.conf` permanecem ativos

## Handoff

- **Settled mid-build:** 2026-11-16 - user: "entendido pode continuar" (D-69 / AD-074: escopo 100% Web-only em `submodules/AutoReparos.Web`)
- **Dispatched:** batch-worker-1 (C2, C15-C20, returned), batch-worker-2 (C5-C9, returned), batch-worker-3 (C10-C14, returned), verifier-1 (at HEAD `a500dde`, returned PASS, 3/3 mutations killed)
- **Boundary:** S1 a S6 implementados em `submodules/AutoReparos.Web` (`ae5e108`) e `scripts/specs/assert_web_adaptacao.py` (22/22 provas verdes, 92.60% de cobertura nos 32 arquivos novos/alterados, 3/3 mutações mortas)
- **Abandoned:** C6 a C14 do `checks.md` de 13/11 (rotas novas e migration no OS e na Cobrança saíram do escopo por D-69)
- **Walked:** none - telas sem captura visual vinculante; a prova de cada tela é o teste unitário do componente
