# Coerência de Negócio, Identidade Visual e Integrações do Web - checks

Mutations: 3
Build: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web build`
Plan: `.specs/features/fase4/web-coerencia-negocio/plan.md`

18 checks in 5 slices · 2 one-way doors · 0 open, of which 0 block

## Checks

### S1 - Identidade visual compartilhada e remoção de consulta pública

**C1** - Os componentes `ModalDialogComponent` e `AlertBannerComponent` são reutilizados em telas de cadastro e banners, centralizados em `<app-page-container>` e utilizam estritamente variáveis CSS de `styles.css` sem duplicação de modais ou cores hexadecimais `#ED145B`/`#18181C` (AC 1)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/components/modal-dialog/modal-dialog.component.spec.ts --filter "ModalDialogComponent"`
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/components/alert-banner/alert-banner.component.spec.ts --filter "AlertBannerComponent"`

**C2** - As páginas `SagasPageComponent` e `AprovacaoOrcamentoPageComponent` renderizam dentro de `<app-page-container>` utilizando `<app-id-badge>`, `<app-status-badge>`, `<app-custom-select>` e `<app-loading-spinner>` no padrão visual do projeto (AC 2)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/sagas/pages/sagas-page/sagas-page.component.spec.ts --filter "SagasPage layout compartilhado"`
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/portal-publico/pages/aprovacao-orcamento-page/aprovacao-orcamento-page.component.spec.ts --filter "AprovacaoOrcamentoPage layout compartilhado"`

**C3** - A rota `/consulta-publica` e o componente `ConsultaPublicaPageComponent` foram completamente removidos de `app.routes.ts`, `navbar.component.ts`, `home-page.component.ts` e `login-page.component.ts` (AC 3)
Proof: `python3 scripts/specs/assert_web_coerencia.py --check consulta-publica`

### S2 - Contratos reais de Catálogo, Estoque e Orçamento

**C4** - `ServicoService` mapeia `precoUnitario` no GET, exibe valores formatados em moeda (nunca R$ 0,00) e envia `precoUnitario` no POST, omitindo o campo `descricao` quando estiver vazio (AC 4)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/servicos/pages/servicos-page/servicos-page.component.spec.ts --filter "ServicosPage precoUnitario"`

**C5** - `InsumoService` lê `quantidadeDisponivel`, `quantidadeReservada` e `precoUnitario` no GET/entrada, envia `{ nome, precoUnitario, quantidadeInicial }` no POST de criação e `{ quantidade }` em `POST /api/v1/insumos/{id}/entradas` (AC 5)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/insumos/pages/insumos-page/insumos-page.component.spec.ts --filter "InsumosPage estoque"`

**C6** - Apenas o perfil `Administrador` pode visualizar os botões de adicionar serviço e insumo, e o botão de registrar entrada de estoque é ocultado para o perfil `Mecanico` (AC 6)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/servicos/pages/servicos-page/servicos-page.component.spec.ts --filter "ServicosPage permissoes"`
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/insumos/pages/insumos-page/insumos-page.component.spec.ts --filter "InsumosPage permissoes"`

**C7** - `OrcamentoPageComponent` agrupa itens do mesmo catálogo somando suas quantidades, permite edição de quantidade, envia `valor: 0` com lista deduplicada no modo padrão de itens e permite alternar para modo avulso com `valor > 0` sem itens (AC 7)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/orcamento/pages/orcamento-page/orcamento-page.component.spec.ts --filter "OrcamentoPage agrupamento"`

### S3 - Fila de Execução, Etapas e Estados Reais

**C8** - O registro de etapa na fila de execução envia `{ tipo, descricao }` não vazia para `POST /api/v1/execucoes/{id}/etapas`, utilizando o texto preenchido no card ou fallback padrão automático, e indica visualmente na lista de etapas quando for descrição automática (AC 8)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/execucao/pages/fila-execucao-page/fila-execucao-page.component.spec.ts --filter "FilaExecucaoPage descricao etapa"`

**C9** - A fila de execução utiliza `<app-custom-select>` restrito aos 5 valores válidos de `EstadoExecucao` (`Fila`, `EmExecucao`, `Concluida`, `Falhou`, `Cancelada`), consulta via query param `?mecanico=` e exibe etapas, `motivoFalha` e indicador de intervenção requerida (AC 9)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/execucao/pages/fila-execucao-page/fila-execucao-page.component.spec.ts --filter "FilaExecucaoPage filtros"`

**C10** - `OrdemServicoService.obterExecucao` consulta `GET /api/v1/fila-execucao` com todos os 5 status e filtra por igualdade estrita de `ordemServicoId`; o status HTTP 403 (Atendente) é tratado como acesso restrito sem notificação de erro, e no Kanban o Atendente visualiza o banner de acesso restrito (AC 10)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-kanban-page/os-kanban-page.component.spec.ts --filter "OsKanbanPage"`

### S4 - Cadastros (`Veículos`, `Clientes`, `Usuários` e `MaskDirective`)

**C11** - `VeiculosPageComponent` exige `chassi` de 17 caracteres e `renavam` de 11 dígitos no cadastro (`POST /api/v1/veiculos`) e mantém campos-chave somente leitura durante edição via `PUT /api/v1/veiculos/{id}` (AC 11)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/veiculos/pages/veiculos-page/veiculos-page.component.spec.ts --filter "VeiculosPage chassi renavam"`

**C12** - A tela de abertura de OS (`OsNovaPageComponent`) consulta `GET /api/v1/veiculos?clienteId={id}` ao selecionar um cliente, vinculando veículos reais do cliente (AC 12)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-nova-page/os-nova-page.component.spec.ts --filter "OsNovaPage veiculos por cliente"`

**C13** - A diretiva `MaskDirective` com `cpfCnpj` alterna dinamicamente entre formato CPF (até 11 dígitos) e CNPJ (12 a 14 dígitos), e `ClientesPageComponent` remove campo de endereço e trava o documento na edição (AC 13)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/shared/directives/mask.directive.spec.ts --filter "MaskDirective cpfCnpj"`
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/clientes/pages/clientes-page/clientes-page.component.spec.ts --filter "ClientesPage documento imutavel"`

**C14** - A edição de usuários em `UsuariosPageComponent` utiliza `PUT /api/v1/usuarios/{id}` com `{ nomeCompleto, tipo }` numérico (1 Administrador, 2 Atendente, 3 Mecanico), recusa perfis não mapeados, e a rota legada `PATCH .../role` é eliminada de `api-endpoints.ts` (AC 14)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/usuarios/services/usuario.service.spec.ts --filter "UsuarioService"`

### S5 - Jornada Conectada (`OsJornadaStore`) e Catálogo Completo de Erros

**C15** - `OsJornadaStore` persiste exclusivamente IDs (`osId, clienteId, veiculoId, orcamentoId, pagamentoId, atualizadoEm`) no `localStorage` e é limpo no logout (`AuthService.logout`); o Kanban combina `fila-execucao`, `admin/sagas` (Administrador) e os `osId` do atalho revalidados por `GET /api/v1/ordens-servico/{id}`, exibindo ordens em `Recebida` também para o `Atendente` e tratando 403 da fila como restrito e erros genéricos com alerta; e telas de orçamento e pagamento oferecem `<app-custom-select>` com as OS do atalho com fallback de input manual (AC 15)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/store/os-jornada.store.spec.ts --filter "OsJornadaStore"`
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-kanban-page/os-kanban-page.component.spec.ts --filter "OsKanbanPage"`

**C16** - `OsDetalhePageComponent` exibe dados de cliente e veículo com badges dedicados e fallback para IDs se a consulta falhar, obtém `orcamentoId` de `GET /api/v1/admin/sagas/{osId}` para Administrador quando ausente no atalho, e exibe link `/aprovar-orcamento?orcamentoId={id}` (sem dados pessoais em query params); e `/aprovar-orcamento` pré-preenche só o `orcamentoId`, não grava nada no `OsJornadaStore`, e oferece 'Iniciar checkout' tratando `409 checkout_indisponivel` (AC 16)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/ordens-servico/pages/os-detalhe-page/os-detalhe-page.component.spec.ts --filter "OsDetalhePage"`
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/features/portal-publico/pages/aprovacao-orcamento-page/aprovacao-orcamento-page.component.spec.ts --filter "AprovacaoOrcamentoPage"`

**C17** - `error-catalog.ts` mapeia todos os 29 códigos de domínio dos microsserviços e da `AuthLambda`, e `errorInterceptor` extrai as propriedades `detalhe` além de `codigo` e `mensagem`, tratando `erro` da `AuthLambda` como detalhe e nunca como código (AC 17)
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/config/error-catalog.spec.ts --filter "ErrorCatalog"`
Proof: `npx --offline yarn@1.22.22 --cwd submodules/AutoReparos.Web test --watch=false --include src/app/core/http/error.interceptor.spec.ts --filter "ErrorInterceptor"`

**C18** - A suíte completa de testes passa com pelo menos 80% de cobertura de linhas nos arquivos novos e alterados, `assert_web_coerencia.py` valida ausência de `/consulta-publica` e contratos reais, e `yarn build` compila sem erros (AC 18)
Proof: `python3 scripts/specs/assert_web_coerencia.py --check cobertura`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| componentes reutilizáveis compartilhados (2) | `ModalDialogComponent` C1 · `AlertBannerComponent` C1 | - |
| rotas legadas/anônimas removidas (1) | `/consulta-publica` C3 | - |
| campos do catálogo corrigidos (5) | `precoUnitario` em serviços C4 · `descricao` opcional em serviços C4 · `quantidadeDisponivel`/`quantidadeReservada` em insumos C5 · `quantidadeInicial` no cadastro de insumos C5 · `precoUnitario` em insumos C5 | - |
| permissões de catálogo e estoque (2) | `Administrador` único para cadastrar catálogo C6 · `Mecanico` bloqueado de entrada de estoque C6 | - |
| modos de orçamento (2) | modo com itens únicos agrupados e `valor: 0` C7 · modo avulso com `valor > 0` sem itens C7 | - |
| campos e estados da fila de execução (3) | etapa com descrição não vazia C8 · filtro por 5 estados válidos C9 · filtro por query param `?mecanico=` C9 | - |
| campos de cadastro e máscara (5) | `chassi` de 17 caracteres C11 · `renavam` de 11 dígitos C11 · busca de veículos por cliente C12 · máscara dinâmica CPF/CNPJ C13 · usuário via `PUT` C14 | - |
| integração da jornada do cliente (4) | persistência local `OsJornadaStore` C15 · visualização de OS `Recebida` no Kanban C15 · resolução de cliente e veículo no Detalhe da OS C16 · checkout direto em aprovação C16 | - |
| catálogo de erros dos microsserviços (2) | códigos de negócio mapeados C17 · extração de `erro` e `detalhe` C17 | - |
| one-way doors (2) | door 1: `OsJornadaStore` (`autoreparos_os_jornada_v1`) C15 · door 2: remoção de `/consulta-publica` C3 | - |

## Swept

- validation: C4 (preço unitário positivo), C5 (quantidade positiva na entrada), C7 (validação de itens no orçamento), C8 (descrição não vazia na etapa), C11 (tamanho de chassi e renavam), C13 (formato de CPF e CNPJ)
- failure modes: C7 (conflito 422 em orçamento inválido), C8 (422 em etapa inválida), C9 (exibição de falha na execução), C16 (409 checkout indisponível quando OS não concluída), C17 (extração padronizada de erro e detalhe)
- idempotency: C7, C8, C11, C14, C16 (desabilitação de botões durante envio ativo)
- authorization: C6 (perfil Administrador para catálogo e bloqueio de Mecânico para entrada de estoque), C14 (administrador para edição de usuários), C15 (atendente visualizando OS Recebida), C16 (token de cliente para checkout)
- concurrency: C15 (atualização consistente de jornada em `localStorage` no navegador)
- data lifecycle: C3 (remoção total da rota pública obsoleta), C15 (limpeza ou retenção dos 100 registros mais recentes em `OsJornadaStore`)
- dependency failure: C16 (tratamento elegante quando consulta de cliente ou veículo falha no Detalhe da OS)
- state transitions: C9 (5 estados de execução), C10 (preservação de execuções finalizadas), C15 (transição de OS de Recebida até Paga no Kanban)
- observability: existing - rastreamento OpenTelemetry preservado

## Handoff

- **Settled mid-build:** 2026-11-16 - user: "Aprovo o plano revisado da web-coerencia-negocio com a D-71" (D-71 / AD-076)
- **Dispatched:** coordinator (implementing S5 according to frozen revised plan)
- **Boundary:** S1 a S5 em `submodules/AutoReparos.Web`
- **Abandoned:** none
- **Walked:** none
