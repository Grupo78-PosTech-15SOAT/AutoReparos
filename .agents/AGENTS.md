# AGENTS.md - AutoReparos System Architecture & AI Coding Guidelines

> **Project:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Ecosystem:** C# .NET 10, ASP.NET Core Minimal APIs, Entity Framework Core, PostgreSQL 16, Angular 19 (Signals & Standalone), Docker, Kubernetes/Helm, Terraform IaC, GitHub Actions.  
> **Architecture Pattern:** Clean Architecture, Domain-Driven Design (DDD), Vertical Slice Architecture.

---

## 1. Project Overview & Business Context

AutoReparos is an integrated automotive repair shop management system engineered to streamline workshop operations, service orders, customer accounts, vehicle history, inventory control, and budget approvals. Originally developed for the **FIAP Tech Challenge (SOAT)**, the platform emphasizes high availability, resilience, dynamic cloud scalability, and domain-driven software design.

### Core Business Domains (Bounded Contexts)
- **Ordens de Serviço (`OrdensServicos`):** Lifecycle management of service orders (Draft, Sent for Approval, Approved, Rejected, In Progress, Completed, Cancelled). Includes public budget approval links for clients.
- **Clientes (`Clientes`):** Customer profile management supporting both Individual (CPF) and Corporate (CNPJ) entities.
- **Veículos (`Veiculos`):** Vehicle registry linked to customers, storing license plate (Standard and Mercosul format), brand, model, year, and mileage.
- **Serviços (`Servicos`):** Catalog of mechanical and diagnostic services with base hourly rates and estimated labor times.
- **Insumos (`Insumos`):** Inventory and parts management with stock level tracking, unit costs, and stock depletion routines upon order execution.
- **Usuários & Autenticação (`Usuarios` / `Auth`):** System users (Mechanics, Receptionists, Admins) using ASP.NET Core Identity and JWT Bearer authorization.
- **Dashboard & Reporting (`Dashboard`):** Financial summary, pending order metrics, inventory alerts, and performance KPIs.

---

## 2. Solution Architecture & Layer Responsibilities

The codebase follows **Clean Architecture** combined with **Vertical Slice** organization per bounded context.

```
AutoReparos/
├── AutoReparos.Domain/             # Core Domain (Entities, Value Objects, Enums, Interfaces)
├── AutoReparos.Application/        # Use Cases, Application Services, DTOs, Validation
├── AutoReparos.Infra/              # EF Core DbContext, PostgreSQL Repositories, Identity, External APIs
├── AutoReparos.API/                # ASP.NET Core Endpoints, Swagger, OpenTelemetry, DI Setup
├── AutoReparos.Web/                # Angular 19 Web Frontend (Standalone Components, Signals)
├── AutoReparos.Domain.Tests/       # Domain Unit Tests (xUnit, FluentAssertions)
├── AutoReparos.Application.Tests/  # Application Logic Tests (xUnit, Moq)
└── AutoReparos.IntegrationTests/   # API & Database Integration Tests (Testcontainers)
```

### Layer Rules & Dependencies

#### 1. `AutoReparos.Domain` (Pure Domain Layer)
- **Zero External Dependencies:** No framework dependencies (no EF Core, ASP.NET, or external libraries in Domain).
- **Rich Domain Models:** Protect domain invariants inside entity methods (`OrdemServico.Aprovar()`, `Insumo.DecrementarEstoque()`). Avoid anemic models.
- **Value Objects:** Enforce domain rules inside Value Objects (`CPF`, `CNPJ`, `Placa`, `Email`). Value objects must be immutable.
- **Repository Contracts:** Domain defines repository interfaces (`IOrdemServicoRepository`, `IClienteRepository`).

#### 2. `AutoReparos.Application` (Orchestration Layer)
- Depends only on `AutoReparos.Domain`.
- Implements use-case orchestration (`OrdemServicoService`, `ClienteService`, `NotificacaoService`).
- Contains request/response DTOs and mapping logic.
- Handles validation logic before invoking domain actions.

#### 3. `AutoReparos.Infra` (Infrastructure & Persistence)
- Depends on `AutoReparos.Application` and `AutoReparos.Domain`.
- Contains `AppDbContext` (EF Core with PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`).
- Implements domain repository interfaces (`OrdemServicoRepository`, `ClienteRepository`).
- Configures ASP.NET Core Identity, JWT Token Generation, and SendGrid email integration.

#### 4. `AutoReparos.API` (HTTP & Entrypoint)
- Exposes Minimal APIs / Controllers for web consumption.
- Configures Dependency Injection (`DependencyInjectionAPI.cs`).
- Configures OpenTelemetry observability (Jaeger, Prometheus, Loki).
- Global exception handling middleware mapping domain exceptions to standard HTTP error payloads.

#### 5. `AutoReparos.Web` (Angular 19 Frontend)
- Modern Angular 19 application built using **Standalone Components** and **Signals** (`signal()`, `computed()`, `effect()`).
- Modular structure organized by feature: `auth`, `clientes`, `veiculos`, `servicos`, `insumos`, `ordens-servico`, `dashboard`, `portal-publico`, `usuarios`.
- Standardized HTTP Interceptors: `jwtInterceptor` (auth token injection) and `errorInterceptor` (centralized error notifications).

---

## 3. Technology Stack & Technical Requirements

| Area | Technology / Framework | Version / Notes |
| :--- | :--- | :--- |
| **Language** | C# | .NET 10.0 (`<Nullable>enable</Nullable>`) |
| **Framework** | ASP.NET Core Minimal APIs | Web API & RESTful Endpoints |
| **Database** | PostgreSQL | Version 16.x |
| **ORM** | Entity Framework Core | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| **Security** | ASP.NET Core Identity & JWT | Bearer Token Auth |
| **Frontend** | Angular 19 | Standalone Components, Signals, Yarn |
| **Testing** | xUnit, Moq, FluentAssertions, Testcontainers | Unit & Integration Testing |
| **Observability** | OpenTelemetry, Prometheus, Jaeger, Loki | Distributed Tracing & Metrics |
| **Containerization** | Docker & Docker Compose | Multi-container setup |
| **Orchestration** | Kubernetes & Helm | HPA enabled, Ingress Nginx |
| **IaC** | Terraform | AWS EKS, ECR, VPC provisioner |

---

## 4. Code Standards & Architectural Guidelines

### C# / .NET Conventions
1. **Nullable Reference Types:** Strict nullability is enabled. Annotate nullable types properly (`string?`) and avoid using suppressions (`!`) without explicit safety guards.
2. **Asynchronous Programming:** Always use `async` / `await` for I/O operations. Accept `CancellationToken` in service and repository methods.
3. **Domain Naming:** Use domain business concepts in Portuguese for domain entities, properties, and methods (e.g., `OrdemServico`, `Placa`, `AprovarOrcamento`, `CalcularTotal`). Use standard English for infrastructure, patterns, and framework artifacts (e.g., `Repository`, `Service`, `Controller`, `DbContext`).
4. **Exception Handling:** Raise specific domain exceptions (`DomainException`, `EntityNotFoundException`, `BusinessRuleException`). Let the API global exception handler translate them into HTTP status codes (400, 404, 409, 422). Never return HTTP 500 for expected validation failures.
5. **Dependency Injection:** Register dependencies using explicit extensions in `DependencyInjection.cs` (Application) and `DependencyInjectionAPI.cs` (API).

### Angular 19 Conventions
1. **Standalone Components:** Do NOT use legacy `NgModule` declarations. Use `standalone: true` components, directives, and pipes.
2. **State Management with Signals:** Prefer Angular Signals (`signal()`, `computed()`, `effect()`) for component reactivity instead of RxJS `BehaviorSubject` where applicable.
3. **HTTP Interceptors:** Use functional HTTP interceptors (`withInterceptors([jwtInterceptor, errorInterceptor])`).
4. **Form Handling:** Use typed Reactive Forms for all data entry screens with dynamic mask support (`CPF/CNPJ`, `Placa Mercosul`, `Telefone`).

---

## 5. Git & Code Review Workflows

### Branch Naming Conventions
- `feature/<slice-or-feature-name>` - New functionality (e.g., `feature/ordem-servico-desconto`)
- `fix/<bug-description>` - Bug fixes (e.g., `fix/calculo-estoque-insumo`)
- `refactor/<target>` - Code refactoring (e.g., `refactor/cliente-value-objects`)
- `chore/<task>` - CI/CD or tooling updates

### Commit Messages (Conventional Commits)
Format: `<type>(<scope>): <short description>`
- `feat(ordemservico): add discount calculation to budget`
- `fix(insumos): check stock availability before deducting`
- `test(clientes): add unit tests for CPF validation`
- `refactor(web): migrate cliente form component to signals`

### Pull Request & Review Standard
Before marking a task complete or submitting a PR:
1. Ensure solution builds without errors: `dotnet build AutoReparos.slnx`
2. Run all unit and integration tests: `dotnet test AutoReparos.slnx`
3. Ensure Angular frontend builds cleanly: `cd AutoReparos.Web && yarn build`
4. Confirm no hardcoded secrets or passwords exist in code or `.env` files.
5. Verify SonarQube quality gate standards are met.

---

## 6. Build, Run & Verification Commands

### Local Development Commands

```bash
# Build .NET Solution
dotnet build AutoReparos.slnx

# Run Unit and Integration Tests
dotnet test AutoReparos.slnx

# Run API Project locally
dotnet run --project AutoReparos.API/AutoReparos.API.csproj

# Build Angular Frontend
cd AutoReparos.Web
yarn install
yarn build
yarn test --watch=false

# Full Docker Compose Execution (PostgreSQL, API, Web, Observability)
docker-compose up -d --build
```

---

## 7. AI Agent Execution Protocol

When executing tasks on this repository:
1. **Inspect Before Mutating:** Read existing target files and tests to understand existing domain contracts before adding code.
2. **Preserve DDD Boundaries:** Do not add infrastructure code (EF Core annotations, SQL queries) inside `AutoReparos.Domain`.
3. **Verify Every Change:** Never declare success without executing `dotnet build` / `dotnet test` or `yarn build`.
4. **Report Findings:** Clearly report implemented changes, test results, and any residual risks.
