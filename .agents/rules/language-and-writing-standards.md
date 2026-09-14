# Language & Writing Standards Specification (`language-and-writing-standards.md`)

This rule specifies the language rules, ubiquitous language conventions, code naming standards, commit writing styles, and documentation guidelines for the **AutoReparos** repository.

---

## 1. Domain Naming vs Infrastructure Naming (Linguagem Ubíqua)

To maintain alignment between the business context (FIAP / Oficina Mecânica) and Clean Architecture standards:

### A. Domain Layer (Português - PT-BR)
All business concepts, domain entities, value objects, aggregate methods, and domain exceptions must be named using **Ubiquitous Language in Portuguese (PT-BR)**:

- **Entities & Value Objects**: `OrdemServico`, `Cliente`, `Veiculo`, `Servico`, `Insumo`, `Usuario`, `Placa`, `Documento`, `Telefone`, `Email`.
- **Domain Methods**: `Aprovar()`, `Recusar()`, `IniciarDiagnostico()`, `AdicionarServico()`, `DecrementarEstoque()`, `CalcularTotal()`.
- **Domain Enums**: `EStatusOrdemServico` (`Recebida`, `EmDiagnostico`, `AguardandoAprovacao`, `EmExecucao`, `Finalizada`, `Entregue`), `ETipoDocumento` (`CPF`, `CNPJ`).
- **Domain Exceptions**: `InvalidClienteException`, `InvalidOrdemServicoException`, `DomainException`.

### B. Technical & Infrastructure Layers (Inglês - EN-US)
All architectural patterns, framework abstractions, design patterns, infrastructure services, and API controllers must use **Standard Technical English (EN-US)**:

- **Architectural Suffixes**: `Repository`, `Service`, `Controller`, `DbContext`, `Handler`, `Middleware`, `Interceptor`, `Strategy`, `Mapping`.
- **Interfaces**: `IClienteRepository`, `IOrdemServicoRepository`, `IJwtService`, `INotificacaoService`, `IKanbanCardStrategy`.
- **Framework Plumbing**: `DependencyInjection`, `GlobalExceptionHandler`, `PagedRequest`, `PagedResult<T>`, `JwtSettings`.

---

## 2. Commit Message & Pull Request Writing Standards

### A. Commit Messages (Gitmoji + Descrição em Português)
O formato obrigatório para mensagens de commit no repositório é `<gitmoji> <Descrição da ação em Português>` ([`git-workflow.md`](./git-workflow.md)):

- **Padrão Canônico de Referência**: `📝 Adicionando especificação técnica`
- **Exemplos Corretos**:
  - `✨ Adicionando suporte a CORS e envio de Role no Login`
  - `🐛 Removendo modificador readonly na diretiva de mascara`
  - `🔥 Removendo código duplicado da raiz do repositório pai`
  - `📝 Adicionando especificação técnica para tarefas da Fase 3`
  - `🔒 Fixando credenciais e configurações de segurança`
- **Incorreto (fora do padrão)**: `feat(api): add cors`, `fix: bugs`, `ajustado backend`

### B. Pull Request Titles & Descriptions
- **Title**: Formatado estritamente com `<gitmoji> <Descrição da ação em Português>` (ex: `✨ Adicionando suporte a gaveta lateral no kanban` ou `📝 Adicionando especificação técnica`).
- **Body**: Detailed markdown in Portuguese (PT-BR) using the PR template ([`PULL_REQUEST_TEMPLATE.md`](../skills/git-pr/templates/PULL_REQUEST_TEMPLATE.md)).

---

## 3. Code Documentation & Commenting Rules

1. **Preserve Comments**: Do not remove existing docstrings, summary comments (`/// <summary>`), or inline explanations.
2. **Self-Documenting Code**: Prefer clean, expressive domain method names (`orfeamServico.AguardarAprovacao()`) over verbose inline comments.
3. **XML Doc Summaries**: Mandatory for public contracts in `AutoReparos.Domain` and `AutoReparos.Application` use cases:
   ```csharp
   /// <summary>
   /// Aprova a ordem de serviço e altera seu status para EmExecucao.
   /// </summary>
   public void Aprovar()
   ```

---

## 4. Summary Matrix

| Artifact | Language | Example |
| :--- | :--- | :--- |
| Domain Entities & Business Methods | **Portuguese (PT-BR)** | `OrdemServico.IniciarDiagnostico(...)` |
| Value Objects & Rules | **Portuguese (PT-BR)** | `Documento.Create(cpf)` |
| Repository Interfaces & Implementations | **English (EN-US) + PT Entity** | `IClienteRepository`, `ClienteRepository` |
| Application Use Cases | **Portuguese (PT-BR) + EN UseCase** | `CriarOrdemServicoUseCase` |
| API Endpoints & Controllers | **Portuguese (PT-BR) + EN Controller** | `ClienteController`, `ClienteEndpoint` |
| Git Commits & PR Descriptions | **Gitmoji + Descrição em Português** | `📝 Adicionando especificação técnica` |
