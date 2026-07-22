# 🚗 AutoReparos — Frontend (Angular 20 & Vertical Slice)

> **Sistema Integrado de Atendimento, Diagnóstico e Execução de Serviços de Oficina Mecânica**  
> Projeto desenvolvido para o **Tech Challenge — FIAP (SOAT)**.

---

## 📌 Visão Geral e Contexto do Projeto

O **AutoReparos Frontend** é a interface web moderna, responsiva e de alta performance projetada para operar em integração com o backend REST API em **.NET 10 (`http://localhost:8080`)** e **PostgreSQL**.

O objetivo do sistema é digitalizar a operação completa de uma oficina mecânica de médio porte, desde o atendimento inicial na recepção até a execução técnica no box de oficina e a entrega do veículo ao cliente, substituindo planilhas manuais e papéis por um fluxo digital rastreável e em tempo real.

---

## 🏛️ Decisões de Arquitetura (Vertical Slice Architecture)

A aplicação foi desenvolvida seguindo os princípios de **Vertical Slice Architecture** (Arquitetura em Fatias Verticais) combinada com os recursos mais modernos do **Angular 20**:

### 🎯 Principais Padrões Utilizados:
1. **Vertical Slices (`src/app/features/`):** Cada funcionalidade do domínio é uma fatia vertical independente, contendo seus próprios modelos de dados (`models/`), serviços de integração HTTP (`services/`), componentes e páginas (`pages/`).
2. **Standalone Components:** Todos os componentes do Angular 20 utilizam a arquitetura *Standalone*, eliminando a necessidade de `NgModules` complexos e reduzindo o acoplamento.
3. **Reatividade com Angular Signals:** Gerenciamento de estado reativo leve para estado de usuário, toasts, notificações e spinners de carregamento.
4. **Proteção e Roteamento Seguro:** Roteamento com *Lazy Loading*, `authGuard` (exige autenticação JWT) e `roleGuard` (controle granular de acesso por perfil).
5. **Pipelines Globais de Tratamento de Erros:**
   - **`HttpErrorInterceptor`:** Trata falhas de rede, 400 Bad Request (toasts de validação), 401 Unauthorized (redirecionamento automático), 403 Forbidden (acesso negado) e 500 Server Error.
   - **`GlobalErrorHandler`:** Captura exceções JS/TS não tratadas na aplicação.

---

## 🎨 Identidade Visual e Estética (FIAP Brand Theme Formal)

A interface adota a estética oficial corporativa da **FIAP**, combinando sofisticação, legibilidade técnica e alto contraste:

```text
               PALETA DA MARCA (FIAP INSTITUTIONAL FORMAL)
┌──────────────────────────────────────────────────────────────┐
│  PRIMARY BRAND:    #ED145B (Official FIAP Pink)              │
│  BACKGROUND DARK:  #0A0A0C (Deep Carbon Black)               │
│  SURFACE DARK:     #18181C (Glassmorphic Slate Black)        │
│  BORDER PINK:      rgba(237, 20, 91, 0.3)                   │
│  MONOSPACE DATA:   #E2E8F0 (Crisp Platinum Silver)           │
└──────────────────────────────────────────────────────────────┘
```

- **Fundo Principal (Carbon Black):** `#0A0A0C`
- **Superfícies e Cards (Glassmorphism):** `#18181C` com bordas em `rgba(255, 255, 255, 0.08)` e brilhos rosados `rgba(237, 20, 91, 0.12)`.
- **Cor Primária de Destaque:** `#ED145B` (Rosa FIAP)
- **Tipografia:**
  - **Outfit (700/800):** Títulos principais, cartões e branding.
  - **Inter (400/500/600):** Corpo de texto, formulários e tabelas.
  - **JetBrains Mono (Silver):** Placas de veículos (`ABC1D23`), CPF/CNPJ, códigos de OS e valores monetários (`R$`).

---

## 📂 Estrutura de Pastas e Fatias Verticais

```text
AutoReparos-Frontend/
├── angular.json
├── package.json
├── tsconfig.json
└── src/
    ├── main.ts
    ├── index.html
    ├── styles.css
    └── app/
        ├── app.component.ts
        ├── app.routes.ts
        ├── app.config.ts
        │
        ├── core/                                 # 🛡️ Infraestrutura Transversal
        │   ├── auth/                             # AuthGuard & RoleGuard
        │   ├── http/                             # JwtInterceptor, LoadingInterceptor & ErrorInterceptor
        │   ├── error/                            # GlobalErrorHandler
        │   └── ui/                               # NotificationService (Toasts) & LoadingService (Spinner)
        │
        ├── shared/                               # 🧩 Design System & Elementos Reutilizáveis
        │   ├── components/                       # NavbarComponent, ToastComponent, LoadingSpinnerComponent, StatusBadgeComponent
        │   ├── directives/                       # MaskDirective (CPF/CNPJ, Placa, Telefone)
        │   └── pipes/                            # StatusOSPipe, CpfCnpjPipe, PlacaPipe
        │
        └── features/                             # 🍕 VERTICAL SLICES (Funcionalidades)
            ├── auth/                             # Slice 1: Autenticação & Login
            │   ├── models/auth.model.ts
            │   ├── services/auth.service.ts
            │   └── pages/login-page/
            │
            ├── ordens-servico/                   # Slice 2: Ordens de Serviço (Core da Oficina)
            │   ├── models/ordem-servico.model.ts
            │   ├── services/ordem-servico.service.ts
            │   ├── components/os-card/           (Card Kanban reutilizável)
            │   └── pages/
            │       ├── os-kanban-page/           (Fila Kanban ordenada por prioridade)
            │       ├── os-lista-page/            (Tabela de busca e ações rápidas)
            │       ├── os-nova-page/             (Abertura de OS com busca de cliente/veículo em tempo real)
            │       └── os-detalhe-page/          (Workbench do Mecânico: diagnóstico, adição de peças e serviços)
            │
            ├── clientes/                         # Slice 3: Gestão de Clientes (PF/PJ)
            │   ├── models/cliente.model.ts
            │   ├── services/cliente.service.ts
            │   └── pages/clientes-page/
            │
            ├── veiculos/                         # Slice 4: Gestão de Veículos e Placas
            │   ├── models/veiculo.model.ts
            │   ├── services/veiculo.service.ts
            │   └── pages/veiculos-page/
            │
            ├── insumos/                          # Slice 5: Controle de Estoque & Alerta de Estoque Mínimo
            │   ├── models/insumo.model.ts
            │   ├── services/insumo.service.ts
            │   └── pages/insumos-page/
            │
            ├── servicos/                         # Slice 6: Catálogo de Serviços de Mão de Obra
            │   ├── models/servico.model.ts
            │   ├── services/servico.service.ts
            │   └── pages/servicos-page/
            │
            ├── usuarios/                         # Slice 7: Gestão de Contas e Níveis de Acesso (Roles)
            │   ├── models/usuario.model.ts
            │   ├── services/usuario.service.ts
            │   └── pages/usuarios-page/
            │
            ├── dashboard/                        # Slice 8: Dashboard Gerencial Administrador
            │   ├── services/dashboard.service.ts
            │   └── pages/dashboard-page/
            │
            └── portal-publico/                   # Slice 9: Portal Público para Clientes
                └── pages/
                    ├── consulta-publica-page/    (Consulta de status por Placa ou CPF sem login)
                    └── aprovacao-orcamento-page/ (Aprovação/Recusa de orçamento via token 1-clique)
```

---

## 👥 Visões e Perfis de Acesso dos Agentes

| Agente Operacional | Permissões & Rotas de Acesso | Funcionalidades Principais |
| :--- | :--- | :--- |
| **Atendente** | `/ordens-servico/nova`, `/clientes`, `/veiculos`, `/ordens-servico` | Recepção do veículo, cadastro de clientes/veículos e abertura de OS (`Recebida`). |
| **Mecânico** | `/ordens-servico/fila`, `/ordens-servico/:id` | Visualização da Fila Kanban, inserção de laudo técnico, consumo de peças e mão de obra. |
| **Administrador** | Todas as rotas + `/dashboard`, `/insumos`, `/servicos`, `/usuarios` | Visão executiva de faturamento, controle de estoque mínimo e tabela de preços. |
| **Cliente (Público)** | `/consulta-publica`, `/aprovar-orcamento` | Acesso sem necessidade de login para acompanhar status por Placa/CPF e aprovar orçamentos. |

---

## 🚀 Como Executar o Projeto com Yarn

### Pré-requisitos:
- **Node.js:** `v22.x` ou superior
- **Gerenciador de Pacotes:** **Yarn**

### 1. Instalar as Dependências:
```bash
yarn install
```

### 2. Executar o Servidor de Desenvolvimento:
```bash
yarn start
```
Acesse a aplicação em: `<http://localhost:4200>`

### 3. Compilar para Produção (Build Otimizado):
```bash
yarn build
```
Os arquivos otimizados serão gerados no diretório `dist/`.

---

## 👤 Autor e Créditos

Desenvolvido por **José Dotta** ([@JoseMD12](https://github.com/JoseMD12))  
Contato: `josehenriquedotta61@gmail.com`  
**Grupo 78 — FIAP Pós-Tech (15SOAT)**
