# Relatório de Diagnóstico e Auditoria Técnica: Frontend (`AutoReparos.Web`)

> **Projeto:** AutoReparos Web Frontend  
> **Tecnologia:** Angular 19 (Standalone Components, Vite/Vitest, Control Flow Moderno)  
> **Diretrizes de Referência:** [`.agents/AGENTS.md`](../../../../.agents/AGENTS.md) & [`.agents/skills/angular-component/SKILL.md`](../../../../.agents/skills/angular-component/SKILL.md)  
> **Data:** Setembro de 2026  
> **Status Global:** Interface rica e visualmente polida, porém com **graves dessincronizações de contratos HTTP com o backend** e **cobertura de testes unitários quase nula (0.5%)**.

---

## 1. Escala de Avaliação e KPIs do Frontend

A auditoria do frontend baseia-se em 5 dimensões mensuráveis:

| Dimensão | Métrica / KPI | Nível Atual | Diagnóstico Resumido |
| :--- | :--- | :--- | :--- |
| **1. Modernidade Angular 19** | Standalone Components, Control Flow (`@if`, `@for`, `@switch`), OnPush. | **L3 (Excelente)** | 100% dos componentes usam `standalone: true`, `@for`/`@if` nativo e `ChangeDetectionStrategy.OnPush`. |
| **2. Gerenciamento de Estado (Signals)** | Adoção de `signal()`, `computed()` em vez de estado mutável clássico. | **L1 (Parcial)** | `AuthService` e `OsKanbanPageComponent` utilizam Signals com maestria; porém a maioria das páginas usa propriedades mutáveis tradicionais. |
| **3. Formulários & Validação** | Uso de Reactive Forms tipados (`FormGroup`) com máscaras vs `[(ngModel)]`. | **L1 (Inconforme)** | O padrão estipulado no `AGENTS.md` (Reactive Forms tipados) não foi seguido: páginas usam template-driven forms com `[(ngModel)]`. |
| **4. Sincronismo de Contratos (API Sync)** | Conformidade exata de URLs, métodos HTTP e DTOs com o backend Minimal APIs. | **L0 (Crítico)** | Múltiplos endpoints usam métodos HTTP errados (`POST`/`PUT` vs `PATCH`), URLs inexistentes e DTOs incompatíveis, gerando erros 400, 404 e 405. |
| **5. Suíte de Testes (Vitest)** | Cobertura de componentes, serviços, interceptors e guards. | **L0 (Crítico)** | Apenas 1 arquivo de teste (`app.spec.ts`) com 2 testes no projeto inteiro. 0% de cobertura nos módulos de negócio. |

---

## 2. Matriz de Dessincronização de Contratos (Frontend vs. Backend)

Abaixo estão listadas as divergências entre o que os serviços Angular em [`AutoReparos.Web`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/core/config/api-endpoints.ts) disparam e o que os endpoints do ASP.NET Core em [`AutoReparos.API`](../../../../submodules/AutoReparos.App/AutoReparos.API/Endpoints/OrdemServicoEndpoint.cs) esperam:

| Funcionalidade / Tela | Chamada no Frontend (`AutoReparos.Web`) | Implementação Real no Backend (`AutoReparos.API`) | Status em Runtime |
| :--- | :--- | :--- | :---: |
| **Abertura de Nova OS**<br>([`OsNovaPageComponent`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/ordens-servico/pages/os-nova-page/os-nova-page.component.ts)) | Envia `{ clienteId, veiculoId, observacoesIniciais }` via `POST /api/ordem-servico` | `CriarOrdemServicoDto` exige obrigatoriamente `DocumentoCliente` e `PlacaVeiculo`. | ❌ **400 Bad Request**<br>(Falha de validação) |
| **Adicionar Serviço na OS**<br>([`OsDetalhePageComponent`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/ordens-servico/pages/os-detalhe-page/os-detalhe-page.component.ts)) | Envia `{ servicoId }` | `AdicionarServicoDto` exige obrigatoriamente `ValorCobrado > 0`. | ❌ **400 Bad Request**<br>(ValorCobrado ausente) |
| **Adicionar Insumo na OS**<br>([`OsDetalhePageComponent`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/ordens-servico/pages/os-detalhe-page/os-detalhe-page.component.ts)) | Envia `{ insumoId, quantidade }` | `AdicionarInsumoDto` exige obrigatoriamente `Descricao` e `Origem` (Estoque ou Compra). | ❌ **400 Bad Request**<br>(Campos obrigatórios faltando) |
| **Consulta Pública de OS**<br>([`ConsultaPublicaPageComponent`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/portal-publico/pages/consulta-publica-page/consulta-publica-page.component.ts)) | Envia query param `?termo=ABC1234` | Backend espera `?documento=...` ou `?placa=...` (`OrdemServicoConsultaPagedRequest`). | ❌ **Retorna lista vazia**<br>(Ignora o termo) |
| **Aprovar/Recusar via Token**<br>([`AprovacaoOrcamentoPageComponent`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/portal-publico/pages/aprovacao-orcamento-page/aprovacao-orcamento-page.component.ts)) | Envia `POST /api/ordem-servico/aprovar` com `{ token, aprovado }` | Backend implementa `GET /api/ordem-servico/aprovar?token=...` e `GET /recusar?token=...`. | ❌ **405 Method Not Allowed** |
| **Enviar para Aprovação**<br>([`ordem-servico.service.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/ordens-servico/services/ordem-servico.service.ts)) | Envia `POST /{id}/enviar-para-aprovacao` | Backend mapeia com `group.MapPatch("/{id}/enviar-para-aprovacao")`. | ❌ **405 Method Not Allowed** |
| **Iniciar Diagnóstico**<br>([`ordem-servico.service.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/ordens-servico/services/ordem-servico.service.ts)) | Envia `PUT /{id}/iniciar-diagnostico` com body `{ observacoes }` | Backend mapeia com `PATCH /{id}/iniciar-diagnostico` sem body. | ❌ **405 Method Not Allowed** |
| **Entregar Veículo**<br>([`ordem-servico.service.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/ordens-servico/services/ordem-servico.service.ts)) | Envia `POST /{id}/entregar` | Backend mapeia com `PATCH /{id}/entregar`. | ❌ **405 Method Not Allowed** |
| **Concluir Serviço da OS**<br>([`ordem-servico.service.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/ordens-servico/services/ordem-servico.service.ts)) | Envia `PUT /{osId}/servicos/{itemId}/concluir` | Backend mapeia com `PATCH /{osId}/servicos/{itemId}/concluir` sem body. | ❌ **405 Method Not Allowed** |
| **Alterar Status Genérico da OS**<br>([`ordem-servico.service.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/ordens-servico/services/ordem-servico.service.ts)) | Envia `PATCH /api/ordem-servico/{id}/status` com `{ novoStatus }` | **Não existe essa rota no backend.** Transições de status são por ações explícitas. | ❌ **404 Not Found** |
| **Atualizar Perfil de Usuário**<br>([`usuario.service.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/usuarios/services/usuario.service.ts)) | Envia `PATCH /api/usuarios/{id}/role` com `{ role }` | **Não existe essa rota no backend.** Backend mapeia `PUT /api/usuarios/{id}` com `UsuarioUpdateDto`. | ❌ **404 Not Found** |

---

## 3. Inspeção Detalhada por Módulo do Frontend

### 3.1. Módulo `OrdensServicos`
* **[`os-kanban-page.component.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/ordens-servico/pages/os-kanban-page/os-kanban-page.component.ts):**
  * Implementação de alta qualidade técnica: uso de `signal`, polling reativo com cancelamento via `stopPolling$`, renderização dinâmica de cards conforme `$type` e gaveta lateral de detalhes (`OsDrawerComponent`).
  * *Ponto de Atenção:* Consome o endpoint `/api/ordem-servico/kanban`, que no backend faz split queries e retorna as colunas estruturadas. Funciona perfeitamente.
* **[`os-nova-page.component.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/ordens-servico/pages/os-nova-page/os-nova-page.component.ts):**
  * Possui salvamento automático de rascunho em `localStorage` e skeleton loaders bem construídos.
  * *Bug Crítico:* O formulário de envio não preenche `DocumentoCliente` nem `PlacaVeiculo`, impossibilitando a abertura de qualquer OS por esta tela.
* **[`os-detalhe-page.component.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/ordens-servico/pages/os-detalhe-page/os-detalhe-page.component.ts):**
  * *Bug de Renderização OnPush:* O método `carregarOS()` executa subscribe sem disparar `cdr.markForCheck()` ou `cdr.detectChanges()`. Com `ChangeDetectionStrategy.OnPush`, a tela pode não atualizar quando os dados chegam da API.
  * As ações de avanço de status chamam o endpoint inexistente `PATCH /api/ordem-servico/{id}/status`.

### 3.2. Módulo `Portal Público`
* **[`consulta-publica-page.component.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/portal-publico/pages/consulta-publica-page/consulta-publica-page.component.ts):**
  * Permite ao cliente consultar a OS sem login através de placa ou CPF.
  * *Bug Crítico:* Passa o parâmetro `termo` na query string. O backend só filtra se receber `documento` ou `placa`. Como o backend não reconhece `termo`, a busca sempre retorna 0 resultados.
  * *Solução:* No service, verificar se o termo possui 7 caracteres alfanuméricos (Placa) ou 11 dígitos (CPF) e preencher os parâmetros corretos: `params.set(isPlaca ? 'placa' : 'documento', termo)`.
* **[`aprovacao-orcamento-page.component.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/portal-publico/pages/aprovacao-orcamento-page/aprovacao-orcamento-page.component.ts):**
  * Permite colar o token recebido por e-mail para aprovar ou recusar o orçamento.
  * *Bug Crítico:* Dispara `POST` com JSON, enquanto o backend implementa `GET /api/ordem-servico/aprovar?token=...` e `GET /recusar?token=...`.

### 3.3. Módulo `Clientes` e `Veículos`
* **[`clientes-page.component.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/clientes/pages/clientes-page/clientes-page.component.ts):**
  * Contém máscaras de CPF/CNPJ e Telefone funcionais via `MaskDirective`.
  * Paginação estruturada e modal de cadastro.
  * *Oportunidade:* Não possui exibição nem edição do campo `Status` (Ativo/Inativo/Bloqueado), que precisará ser adicionado para suportar o Ponto 1 do Backend.

### 3.4. Módulo `Dashboard`
* **[`dashboard-page.component.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/dashboard/pages/dashboard-page/dashboard-page.component.ts):**
  * Integração com Chart.js para histórico mensal de 6 meses (OSs, faturamento e serviços).
  * KPIs visuais e alertas de insumos com estoque baixo.
  * Funciona corretamente com a API `/api/dashboard/metrics`.

---

## 4. Diagnóstico de Build e Testes Vitest

### 4.1. Compilação de Produção (`yarn build`)
* **Resultado:** **Aprovado (Sucesso)**.
* **Tempo de build:** ~7.1 segundos.
* **Bundle:** `main` (86 kB transfer size), chunks lazy gerados adequadamente. O bundle cumpre os limites de orçamento do `angular.json`.

### 4.2. Suíte de Testes Unitários (`yarn test --watch=false`)
* **Comando executado:** `yarn ng test --watch=false`
* **Resultado:**
  ```text
  Test Files: 1 passed (1)
       Tests: 2 passed (2)
    Duration: 992ms
  ```
* **Diagnóstico de Cobertura:**
  * O único arquivo de teste é [`src/app/app.spec.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/app.spec.ts), que apenas instancia o `AppComponent` raiz e verifica se o título é renderizado.
  * **Nenhuma página, serviço, pipe ou interceptor possui teste unitário.**
  * A cobertura de testes do frontend é inferior a **1%**.

---

## 5. Plano de Ação para Adequação do Frontend

### Fase 1: Correção Imediata dos Contratos de API (Hotfixes)
1. **Corrigir `ordem-servico.service.ts`:**
   * Ajustar métodos para usar `PATCH` em vez de `POST`/`PUT` nas transições de fluxo (`enviarParaAprovacao`, `iniciarDiagnostico`, `entregar`, `concluirServico`).
   * Substituir `atualizarStatus` por métodos específicos para cada ação.
   * Ajustar `responderOrcamentoToken` para chamar `GET /api/ordem-servico/aprovar?token=...` ou `GET /recusar?token=...`.
2. **Corrigir `OsNovaPageComponent`:**
   * Localizar o cliente selecionado para extrair o `documento` e o veículo para extrair a `placa`.
   * Enviar `DocumentoCliente` e `PlacaVeiculo` no payload de abertura da OS.
3. **Corrigir `ConsultaPublicaPageComponent`:**
   * Ajustar o método de busca para enviar `placa` ou `documento` conforme o padrão digitado.

### Fase 2: Evolução de UI para Fase 3 (Status do Cliente)
1. Atualizar [`cliente.model.ts`](../../../../submodules/AutoReparos.App/AutoReparos.Web/src/app/features/clientes/models/cliente.model.ts) com o campo `status: 'Ativo' | 'Inativo' | 'Bloqueado'`.
2. Adicionar badge de status na tabela de clientes e opção de inativação/reativação.

### Fase 3: Criação da Suíte de Testes Unitários com Vitest
1. Criar specs para os serviços críticos (`auth.service.spec.ts`, `ordem-servico.service.spec.ts`, `cliente.service.spec.ts`).
2. Criar specs para os interceptors (`jwt.interceptor.spec.ts`, `error.interceptor.spec.ts`).
3. Criar specs para componentes principais (`login-page.component.spec.ts`, `os-kanban-page.component.spec.ts`, `os-nova-page.component.spec.ts`).
