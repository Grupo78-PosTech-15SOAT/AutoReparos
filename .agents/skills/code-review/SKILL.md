---
name: code-review
description: Comprehensive code review checklist, static analysis verification, architectural compliance checker, and test quality auditor for AutoReparos.
---

# Code Review Skill (`code-review`)

This skill provides an in-depth code review methodology for the **AutoReparos** codebase. It covers Clean Architecture & DDD compliance verification, static analysis & Roslyn warning inspection, test suite auditing, and .NET 10 / Angular 19 best practices.

---

## Step-by-Step Review Workflow

### Step 1: Architectural Compliance Audit (Clean Architecture & DDD)

Inspect code changes across layer boundaries to enforce clean separation:

```text
AutoReparos.Domain  <--  AutoReparos.Application  <--  AutoReparos.Infra
        ^                                                    ^
        +------------------ AutoReparos.API -----------------+
```

#### A. Domain Layer (`AutoReparos.Domain`)
- **Isolation Check:** Verify ZERO references to external frameworks (`Microsoft.AspNetCore`, `Microsoft.EntityFrameworkCore`, `Npgsql`, SendGrid, HTTP clients).
- **Domain Invariants:** Ensure aggregate roots and entities control state changes through explicit methods, not public setters.
- **Value Objects:** Ensure immutability for value objects (e.g., CPF, Email, Currency/Price).

#### B. Application Layer (`AutoReparos.Application`)
- **Use Case Handlers:** Verify application handlers coordinate domain operations and call repository interfaces.
- **DTO Encapsulation:** Verify domain entities are NOT exposed directly in API contracts; request/response DTOs must be used.
- **Validation:** Check input validation logic before domain execution.

#### C. Infrastructure Layer (`AutoReparos.Infra`)
- **Repository Implementation:** Ensure repositories handle persistence via EF Core and return domain entities.
- **Database Context & Configurations:** Verify EF Core entity configurations use Fluent API in `Infra/Data/Configurations` rather than polluting Domain classes with data annotations.
- **External Integration:** Verify SendGrid, Identity, and JWT services implement interfaces defined in Application/Domain layers.

#### D. Presentation & API Layer (`AutoReparos.API` & `AutoReparos.Web`)
- **API Endpoints:** Controller actions and Minimal APIs should remain thin: delegate execution immediately to application handlers.
- **Angular 19 Frontend:** Verify use of Signals, Standalone Components, strict TypeScript types, and proper RxJS unsubscribing.

---

### Step 2: Static Analysis & Quality Gate Verification

1. **Roslyn Warnings & Compilation Quality:**
   Run solution build with warning verification:
   ```bash
   dotnet build AutoReparos.slnx -c Release /p:TreatWarningsAsErrors=true
   ```
2. **SonarQube Quality Gate Criteria:**
   Inspect `sonar-project.properties` and verify compliance:
   - Zero critical or blocker bugs.
   - Code coverage threshold maintained on new/modified code.
   - Duplicated blocks kept below threshold.
   - Cognitive complexity within acceptable limits (method complexity < 15).

---

### Step 3: Test Coverage & Quality Audit

1. **Domain Tests (`AutoReparos.Domain.Tests`):**
   - Verify unit tests exist for all new or modified domain methods.
   - Verify edge cases, boundary conditions, and domain exceptions are asserted.
2. **Application Tests (`AutoReparos.Application.Tests`):**
   - Verify handler unit tests use mock/stub repositories correctly.
3. **Integration Tests (`AutoReparos.IntegrationTests`):**
   - Verify end-to-end integration tests exist for new API endpoints and PostgreSQL operations.
4. **Test Structure Check:**
   - Confirm tests follow the **Arrange-Act-Assert (AAA)** pattern.
   - Ensure tests are deterministic and do not depend on static global state or external network calls.

---

### Step 4: Performance & C# / .NET 10 Best Practices

- **Async/Await Usage:**
  - All I/O operations (database queries, network requests, file access) must be asynchronous (`async`/`await`).
  - No blocking sync-over-async (`.Result`, `.Wait()`, `Task.Run().Result`).
  - `CancellationToken` forwarded through application and infrastructure layers.
- **EF Core Efficiency:**
  - Read-only endpoints use `.AsNoTracking()`.
  - Projections (`.Select(...)`) used to retrieve only required fields rather than full entities when mapping to DTOs.
  - No N+1 query loops; use `.Include()` or explicit projections.
- **Resource Management:**
  - Proper disposal using `await using` or `using` for disposable resources (`DbContext`, HTTP response streams).

---

### Step 5: Formulate Code Review Findings

Construct a structured Code Review Markdown report:

```markdown
# 📋 Code Review Report

**Target Scope:** `<modules / files reviewed>`
**Overall Quality Rating:** `[ EXCELLENT | GOOD | NEEDS_IMPROVEMENT | REJECTED ]`

---

## 🏗️ Architectural Compliance Checklist
- [x] **Domain Purity:** Domain layer clean of infrastructure dependencies.
- [x] **Layer Boundaries:** Application handlers properly isolate domain entities.
- [x] **Infrastructure Encapsulation:** Repositories and EF Core configurations isolated.
- [x] **Presentation Separation:** Controllers/Endpoints thin and focused.

---

## 🎯 Findings & Improvement Checklist

| Severity | File Path | Category | Issue & Suggested Fix |
| :--- | :--- | :--- | :--- |
| 🔴 High | `../../../AutoReparos.Domain/OrdensServicos/OrdemServico.cs#L55` | DDD Invariant | Public setter exposed on status. Replace with `AtualizarStatus(...)` method. |
| 🟡 Medium | `../../../AutoReparos.Infra/Repositories/OrdemServicoRepository.cs#L32` | Performance | Query missing `.AsNoTracking()`. Add tracking disable for read-only fetch. |
| 🟢 Low | `../../../AutoReparos.API/Controllers/OrdensServicosController.cs#L18` | Style | Unused namespace import. |

---

## 🧪 Verification Log
- Compiler warnings check: 0 warnings.
- Domain unit test suite: All tests passed.
- Application test suite: All tests passed.
```
