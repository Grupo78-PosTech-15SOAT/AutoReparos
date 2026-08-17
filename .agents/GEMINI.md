# GEMINI.md - Antigravity / Gemini CLI Agent Guidelines

> **Project:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Environment:** Gemini CLI / Antigravity Harness  
> **Target Frameworks:** .NET 10.0 (C#), Angular 19, EF Core, PostgreSQL 16  

---

## 1. Gemini Agent Environment Overview

This document provides specialized guidelines for Gemini CLI and Antigravity Gemini-based agent execution when operating inside the `AutoReparos` repository.

Refer to the primary [`AGENTS.md`](AGENTS.md) for full business context, DDD layer responsibilities, and solution architecture details.

---

## 2. Quick Command Reference for Gemini CLI

### Backend Build & Test Suite
```bash
# Compile solution
dotnet build AutoReparos.slnx

# Run all xUnit tests (Domain, Application, Integration)
dotnet test AutoReparos.slnx

# Run specific test project
dotnet test AutoReparos.Domain.Tests/AutoReparos.Domain.Tests.csproj
dotnet test AutoReparos.Application.Tests/AutoReparos.Application.Tests.csproj
dotnet test AutoReparos.IntegrationTests/AutoReparos.IntegrationTests.csproj
```

### Database & EF Core Migrations
```bash
# Add a new EF Core migration
dotnet ef migrations add <MigrationName> --project AutoReparos.Infra/AutoReparos.Infra.csproj --startup-project AutoReparos.API/AutoReparos.API.csproj

# Update local database
dotnet ef database update --project AutoReparos.Infra/AutoReparos.Infra.csproj --startup-project AutoReparos.API/AutoReparos.API.csproj
```

### Frontend Build & Test Suite
```bash
# Navigate to web frontend
cd AutoReparos.Web

# Install packages
yarn install

# Production build
yarn build

# Run unit tests
yarn test --watch=false
```

---

## 3. Gemini Execution Protocols & Best Practices

### Rule 1: Read and Inspect Before Editing
- Always read existing entity definitions in `AutoReparos.Domain` before creating or modifying application services or endpoints.
- Check Value Object validation logic (e.g., `CPF.cs`, `CNPJ.cs`, `Placa.cs`) to ensure input data formats conform to system invariants.

### Rule 2: Protect Clean Architecture & DDD Isolation
- **Domain Layer (`AutoReparos.Domain`):** Keep free of external libraries. Domain entities encapsulate business rules (`OrdemServico`, `Cliente`, `Veiculo`, `Insumo`, `Servico`).
- **Application Layer (`AutoReparos.Application`):** Place DTOs, use-case handlers, and service orchestrations here.
- **Infrastructure Layer (`AutoReparos.Infra`):** Put EF Core configurations, repository implementations, and Identity/JWT logic here.
- **API Layer (`AutoReparos.API`):** Endpoints should be concise, delegating business logic to Application Services and handling HTTP request mapping.

### Rule 3: Angular 19 Frontend Guidelines
- All new Angular components must be **Standalone** (`standalone: true`).
- Prefer **Angular Signals** (`signal()`, `computed()`, `effect()`) for reactive UI state.
- Ensure API integration goes through typed services in `AutoReparos.Web/src/app/core/services/` or feature services.

### Rule 4: Mandatory Empirical Verification
- **Never claim completion without verifying.**
- Always run `dotnet build AutoReparos.slnx` and `dotnet test AutoReparos.slnx` after backend changes.
- Always run `yarn build` inside `AutoReparos.Web` after frontend changes.

---

## 4. Problem Diagnosis & Log Inspection Strategy

When encountering runtime or build failures:
1. Fetch and inspect the un-truncated log or build output immediately.
2. Identify the root cause line (e.g., missing DI registration, compilation error, or broken test assertion).
3. Fix the underlying root cause directly; do not bypass failing tests or swallow exceptions.
4. Re-run verification commands to confirm resolution.
