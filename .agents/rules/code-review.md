# Code Review & Quality Assurance Rules

This rule outlines the mandatory automated quality gates, security checks, anti-pattern detection rules, and code review criteria for `AutoReparos`.

---

## 1. Quality Gates & Automated Analysis

- **SonarQube / SonarCloud Integration**: `sonar-project.properties` and Sonar analysis must pass with Quality Gate Status **PASSED**.
- **Code Coverage Threshold**: Minimum **80% line coverage** required for `AutoReparos.Domain` and `AutoReparos.Application`.
- **Cyclomatic Complexity**: No single method should exceed a complexity score of **15**. Break complex methods down into smaller, pure helper functions.
- **Compiler Warnings**: Zero build warnings allowed (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in CI release builds).

---

## 2. Security Standards (OWASP Top 10)

### 2.1 SQL Injection Prevention
- **Strict Prohibition**: Never concatenate raw strings inside SQL statements (`FromSqlRaw($"SELECT * FROM Clientes WHERE Nome = '{input}'")`).
- **Required Practice**: Use EF Core LINQ methods or parameterized queries (`FromSqlInterpolated` / parameterized parameters).

### 2.2 Sensitive Data & Secrets Management
- **Never Commit Secrets**: Hardcoded connection strings, API keys (e.g. SendGrid keys), or JWT signing keys in source code are strictly prohibited.
- **Environment Variables & Secrets**: Use `appsettings.json` placeholders, Environment Variables, or `dotnet user-secrets` for local development.

### 2.3 Authentication & Authorization
- Verify JWT tokens in `AutoReparos.API` endpoints using proper key validation, issuer/audience checks, and lifetime validation.
- All non-public endpoint groups must call `.RequireAuthorization()`.

---

## 3. Anti-Patterns & Code Smells Checklist

During code review or agent self-assessment, flag and refactor any of the following anti-patterns:

### ❌ Anti-Pattern 1: Leaking Domain Entities to API
- **Symptom**: Returning Domain entities directly from Controllers or Endpoints.
- **Risk**: Exposes internal domain invariants, creates circular reference serialization errors, breaks contract isolation.
- **Fix**: Map domain entities to explicit Request/Response DTOs in the Application layer before returning.

### ❌ Anti-Pattern 2: Anemic Domain Model
- **Symptom**: Domain entities with public getters and setters for all properties, with business logic residing entirely in external services.
- **Risk**: Encapsulation loss, duplicate validation logic scattered across services.
- **Fix**: Move business invariants and state mutators into public methods on the Aggregate Root/Entity (`cliente.Atualizar(...)`, `ordemServico.AdicionarServico(...)`).

### ❌ Anti-Pattern 3: N+1 Database Query Pattern
- **Symptom**: Executing database calls inside a `foreach` loop over a collection.
- **Risk**: Catastrophic database load and query latency degradation.
- **Fix**: Batch fetch using `.Where(x => keys.Contains(x.Id))` or project with EF Core `.Include()` / `.Select()`.

### ❌ Anti-Pattern 4: Silent Exception Swallowing
- **Symptom**: Empty `catch (Exception ex)` blocks or swallowing exceptions without logging or handling.
- **Risk**: Hidden runtime failures and un-debuggable state corruption.
- **Fix**: Catch specific exceptions, log structured details via `ILogger`, and handle gracefully or let `GlobalExceptionHandler` format a Problem Details response.

### ❌ Anti-Pattern 5: Floating Async Operations
- **Symptom**: Calling async methods without `await` or not returning the `Task`.
- **Risk**: Unhandled background exceptions crash processes or escape error tracking.
- **Fix**: Always `await` async calls or handle background tasks explicitly.

---

## 4. Code Review Protocol for Reviewers & AI Agents

When reviewing code or validating changes:

1. **Architecture Compliance**:
   - Are layer dependencies respected? (Domain has no dependencies, API delegates to Application UseCases).
   - Is the feature structured as a Vertical Slice in Application?
2. **Correctness & Tests**:
   - Are edge cases (null inputs, empty lists, duplicate keys) covered by unit tests?
   - Do domain unit tests verify entity state changes and invalid input exceptions?
3. **Performance & Data Access**:
   - Is `.AsNoTracking()` applied to read queries?
   - Are database indexes added for newly introduced search columns?
4. **Code Cleanliness**:
   - Are C# 12/13 primary constructors used consistently?
   - Is Angular control flow (`@if`, `@for`) used instead of `*ngIf`/`*ngFor`?
   - Are signals used for reactive state in Angular components?
