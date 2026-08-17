---
name: git-commit
description: Conventional Commits generator, scope checker, pre-commit validation, and secret protection for AutoReparos.
---

# Git Commit Skill (`git-commit`)

This skill provides step-by-step procedures for validating working tree state, selecting conventional commit scopes, running pre-commit checks, and generating compliant Conventional Commit messages for the **AutoReparos** repository.

---

## Step-by-Step Procedure

### Step 1: Staging & Diff Inspection
1. Inspect current working tree state:
   ```bash
   git status
   ```
2. Inspect staged changes to ensure only intended files are staged:
   ```bash
   git diff --staged
   ```
3. **Secrets Audit (Mandatory):**
   Verify that no sensitive files or credentials are staged:
   - File checks: `.env`, `local.json`, `appsettings.Local.json`, secrets JSON files, `.pem` keys.
   - Code pattern checks: Hardcoded passwords, JWT secrets, SendGrid API keys, AWS credentials.
   - If any secret is detected, abort immediately and unstage the file:
     ```bash
     git reset HEAD <file>
     ```

---

### Step 2: Scope Resolution & Validation
Determine the scope based on the primary directory touched in the diff:

| Scope | Directory / Pattern | Description |
| :--- | :--- | :--- |
| `api` | `AutoReparos.API/` | API endpoints, controllers, middleware, Swagger |
| `app` | `AutoReparos.Application/` | Use cases, handlers, DTOs, application services |
| `domain` | `AutoReparos.Domain/` | Domain entities, value objects, domain events, rules |
| `infra` | `AutoReparos.Infra/` | Entity Framework, PostgreSQL, Identity, SendGrid |
| `web` | `AutoReparos.Web/` | Angular 19 frontend components, services, styles |
| `tests` | `*.Tests/`, `IntegrationTests/` | Unit, application, or integration test files |
| `k8s` | `k8s/` | Kubernetes manifests, Helm values and templates |
| `tf` | `infra/` | Terraform infrastructure modules and configs |
| `scripts` | `scripts/` | Shell scripts, dev environment helpers |
| `deps` | `package.json`, `.csproj`, `sln` | Package updates or solution structure changes |

*Note:* If changes span multiple modules, choose the scope representing the core functional intent, or omit scope if it affects repository-wide rules (`chore: update root .gitignore`).

---

### Step 3: Conventional Commit & Gitmoji Type Selection

Select the commit type and optional Gitmoji according to the repository standards:

- `✨` / `feat`: New feature or capability (`✨ (web): adicionar filtro de ordens de serviço por cliente`)
- `🐛` / `fix`: Bug fix (`🐛 (infra): corrigir recalculo de estoque ao cancelar ordem`)
- `🚨` / `test` / `fix`: Code smell or SonarQube fix (`🚨 (app): resolver Code Smells apontados pelo SonarQube`)
- `🎨` / `style` / `refactor`: UI adjustment, contrast, or refactoring (`🎨 (web): ajuste de contraste WCAG AA`)
- `🔒` / `security`: Security adjustment (`🔒 (infra): ajuste de seguranca Nginx para container sem root`)
- `🚚` / `refactor`: Structural file move or rename (`🚚 (web): renomeando pasta do frontend para AutoReparos.Web`)
- `📝` / `docs`: Documentation update (`📝 (readme): atualizar secao de arquitetura no README`)
- `👷` / `ci`: CI/CD pipeline changes (`👷 (ci): ajustar trigger do workflow de deploy no GitHub Actions`)
- `chore`: Dependency update or repo tooling adjustment (`chore: atualizar pacotes NuGet`)

---

### Step 4: Pre-Commit Validation Runner
Before committing, execute compilation and verification checks:

1. **Backend Validation (.NET 10):**
   ```bash
   dotnet build AutoReparos.slnx -c Release
   ```
2. **Frontend Validation (Angular 19 - if `AutoReparos.Web` changed):**
   ```bash
   cd AutoReparos.Web && yarn build && cd ..
   ```
3. **Execute Affected Tests:**
   ```bash
   dotnet test AutoReparos.Domain.Tests/AutoReparos.Domain.Tests.csproj -c Release
   dotnet test AutoReparos.Application.Tests/AutoReparos.Application.Tests.csproj -c Release
   ```

*Shortcut:* Run the automated validation script:
```bash
bash .agents/skills/git-commit/scripts/validate-commit.sh
```

---

### Step 5: Format & Execute Commit
Construct a concise commit message following this schema:

```text
<gitmoji> <type>(<scope>): <short summary in imperative mood>

[optional body explaining motivation and changes]
```

#### Real Repository Commit Examples:
- `✨ (web): suporte a CORS, envio de Role no Login e integracao monorepo frontend`
- `🐛 (infra): removendo modificador readonly do metodo applyMask na diretiva de mascara`
- `🔒 (docker): ajuste de seguranca Nginx para rodar sem root (USER nginx) na porta 8080`
- `📝 (docs): adicionando secao de Arquitetura Frontend Angular no README`
- `🚨 (sonar): resolução de apontamentos adicionais de Medium/Low do SonarQube`

#### Commit Execution Command:
```bash
git commit -m "✨ (web): suporte a CORS, envio de Role no Login e integracao monorepo frontend"
```

---

### Step 6: Post-Commit State Verification
Confirm the commit succeeded and working tree is in a clean state:
```bash
git status
git log -1 --stat
```
