---
name: git-review
description: Execute git diff & Pull Request code reviews, security vulnerability audits, breaking change detection, and architectural diff analysis for AutoReparos.
---

# Git Review Skill (`git-review`)

This skill provides an automated, structured process for reviewing Git diffs, branch comparisons, and Pull Requests in the **AutoReparos** repository. It focuses on security audits, breaking change detection, git diff analysis, and generating actionable code review feedback.

---

## Step-by-Step Procedure

### Step 1: Extract Git Diff & Context
Determine the target diff source:

1. **Review Local Branch vs `develop`:**
   ```bash
   git diff origin/develop...HEAD
   ```
2. **Review Specific Commit Range:**
   ```bash
   git diff HEAD~1..HEAD
   ```
3. **Review GitHub Pull Request (using `gh` CLI):**
   ```bash
   gh pr view <pr-number>
   gh pr diff <pr-number>
   ```

Extract modified file list:
```bash
git diff --name-status origin/develop...HEAD
```

---

### Step 2: Automated Security Audit
Scan the diff against critical security vectors:

#### A. Secrets & Sensitive Information Leakage
- Inspect diff lines for hardcoded credentials, JWT secrets, SendGrid API keys, database connection strings (`Host=...;Password=...`), AWS keys.
- Search for accidental staging of `.env`, `appsettings.Local.json`, or `.pem` files.

#### B. SQL & Entity Framework Injection Risks
- Search diff for raw SQL execution: `FromSqlRaw`, `ExecuteSqlRaw`, or concatenated strings in SQL statements.
- Verify that parameterized queries or EF Core LINQ methods (`Where`, `FirstOrDefaultAsync`) are used exclusively.

#### C. Authentication & Authorization Enforcement
- Verify all newly added API controllers or endpoints in `AutoReparos.API` specify appropriate `[Authorize]` attributes or policies unless explicitly public.
- Check ASP.NET Core Identity integration for token validation and role-based access checks.

#### D. Unsanitized Input & OWASP Risks
- Check DTO input validations and binding models in `AutoReparos.Application` and `AutoReparos.API`.
- Verify CORS policies, header validations, and sensitive data masking in logs.

---

### Step 3: Breaking Change Detection
Identify potential breaking changes across solution layers:

1. **REST API Contract Breaking Changes (`AutoReparos.API`):**
   - Removed or renamed endpoint routes.
   - Removed properties from response DTOs or changed property types.
   - Modified required query parameters or body payloads.
2. **Database Schema Breaking Changes (`AutoReparos.Infra` / Migrations):**
   - Removed table columns or tables in EF Core migrations.
   - Changed non-nullable columns without fallback default values.
   - Renamed existing database columns or indexes.
3. **Infrastructure & Deployment Breaking Changes (`k8s/`, `infra/`):**
   - Changed Helm `values.yaml` key names required by `deploy.yml`.
   - Modified Terraform outputs or state variables required by CI/CD pipeline.

---

### Step 4: Quality & Diff Analysis

1. **File Churn & Scope Audit:**
   - Are changes tightly scoped to the PR topic?
   - Are unrelated files or white-space reformatting mixed in?
2. **Clean Architecture Boundary Audit:**
   - Does `AutoReparos.Domain` remain free of infrastructure, database, or API dependencies?
   - Are domain invariants enforced within domain entities rather than leaked into handlers/controllers?
3. **Performance & Concurrency Audit:**
   - Check for missing `.AsNoTracking()` on read-only EF Core queries.
   - Check for blocking sync-over-async calls (`.Result`, `.Wait()`).
   - Check for potential N+1 query patterns inside loops.

---

### Step 5: Structured Review Report Generation
Synthesize review findings into a clean Markdown review report using the following standard template:

```markdown
# 🔍 Git & PR Review Report

**Target:** `<branch / PR #>`
**Verdict:** `[ APPROVED | REQUEST_CHANGES | COMMENT ]`

---

## 🚨 Critical Security & Breaking Findings
<!-- Blockers that MUST be resolved before merging -->
- [ ] **[SECURITY/BREAKING]** `file:///path/to/file.cs#L45`: Description of issue.

---

## 🏛️ Architecture & Code Improvements
<!-- Recommendations and code quality suggestions -->
- [ ] **[QUALITY]** `file:///path/to/file.cs#L120`: Suggestion for optimization or readability.

---

## 📋 File-by-File Review Summary
| File Path | Status | Impact Level | Notes |
| :--- | :--- | :--- | :--- |
| `AutoReparos.Domain/OrdensServicos/OrdemServico.cs` | 🟡 Needs Edit | High | Invariant check missing |
| `AutoReparos.API/Controllers/OrdensServicosController.cs` | 🟢 Approved | Low | Clean implementation |

---

## 🧪 Verification Commands Executed
- Build check: `dotnet build AutoReparos.slnx -c Release` -> Passed
- Unit tests: `dotnet test AutoReparos.Domain.Tests` -> Passed
```
