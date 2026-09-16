---
name: git-pr
description: Prepare, format, validate, and create Pull Requests using GitHub CLI (gh pr create) with standard AutoReparos PR templates and pre-PR checklist runner.
---

# Git PR Skill (`git-pr`)

This skill standardizes the Pull Request creation process for the **AutoReparos** project. It enforces branch hygiene, runs pre-PR validation builds and tests, generates structured Markdown descriptions from repository templates, and uses the GitHub CLI (`gh`) to open PRs.

---

## Step-by-Step Procedure

### Step 1: Branch Hygiene & Sync Check
1. Check active branch:
   ```bash
   git branch --show-current
   ```
   *Rule:* Features and fixes must target `develop`. Hotfixes target `main`. Do NOT create PRs originating directly from `main`.
2. Check working directory status:
   ```bash
   git status
   ```
   Ensure all changes are committed before opening a PR.
3. Sync local branch with remote target:
   ```bash
   git fetch origin
   git log HEAD..origin/develop --oneline
   ```
   If local branch is behind, rebase or merge `origin/develop` before creating the PR.

---

### Step 2: Pre-PR Checklist Runner
Run the full verification suite across all solution components:

1. **Build Solution:**
   ```bash
   dotnet build AutoReparos.slnx -c Release
   ```
2. **Execute Complete Test Suite:**
   ```bash
   dotnet test AutoReparos.Domain.Tests/AutoReparos.Domain.Tests.csproj -c Release
   dotnet test AutoReparos.Application.Tests/AutoReparos.Application.Tests.csproj -c Release
   dotnet test AutoReparos.IntegrationTests/AutoReparos.IntegrationTests.csproj -c Release
   ```
3. **Frontend Build Check (if Web files touched):**
   ```bash
   cd AutoReparos.Web && yarn build --configuration development && cd ..
   ```
4. **Terraform Validation (if Infra files touched):**
   ```bash
   cd infra && terraform fmt -check && cd ..
   ```

---

### Step 3: Pull Request Title Formatting
Formulate a clear PR title matching Conventional Commit syntax:

`type(scope): concise description of the feature or fix`

#### Examples:
- `feat(app): implement customer notification service via SendGrid`
- `fix(infra): update PostgreSQL connection string retry policy`
- `refactor(domain): enforce order status transition invariants`

---

### Step 4: Description Generator & Template Execution
Read the template located at `.agents/skills/git-pr/templates/PULL_REQUEST_TEMPLATE.md` and populate all sections:

1. **Summary of Changes**: High-level overview of introduced changes and motivation.
2. **Type of Change**: Select appropriate checkbox (`New Feature`, `Bug Fix`, `Refactoring`, `Database Migration`, `Infra/K8s/Terraform`, `Docs`).
3. **Architectural & Database Impact**:
   - Mention affected layers (`Domain`, `Application`, `Infra`, `API`, `Web`).
   - Detail any EF Core migrations added (e.g., `AddCustomerIndexes`).
   - Detail infrastructure changes (Kubernetes manifests, Helm chart parameters, Terraform IaC).
4. **Testing Matrix**:
   - Unit tests executed (`AutoReparos.Domain.Tests`, `AutoReparos.Application.Tests`).
   - Integration tests executed (`AutoReparos.IntegrationTests`).
   - Manual tests executed (Swagger UI endpoints, Postman collections).
5. **Security & Breaking Changes**:
   - Secrets verification check.
   - Authentication/Authorization rules applied (`[Authorize]`, JWT policies).
   - Breaking API or Database contract disclosures.

---

### Step 5: GitHub CLI (`gh`) PR Creation
Create the Pull Request using GitHub CLI:

```bash
gh pr create \
  --title "feat(app): implement customer notification service via SendGrid" \
  --body-file .agents/skills/git-pr/templates/PULL_REQUEST_TEMPLATE.md \
  --base develop
```

Or pass body directly via command line parameter:
```bash
gh pr create \
  --title "feat(app): implement customer notification service via SendGrid" \
  --body "## Summary of Changes..." \
  --base develop
```

*Note:* If GitHub CLI is unavailable in the environment, output the full formatted PR title and Markdown description block so the developer can paste it directly into GitHub Web UI.

---

### Step 6: Post-Creation Verification
1. Display the created PR URL and ID.
2. Verify CI pipeline triggers (e.g. GitHub Actions `deploy.yml` workflow).
3. Report status back to caller/developer.
