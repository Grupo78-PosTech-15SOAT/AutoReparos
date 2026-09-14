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

### Step 3: Gitmoji Selection

Selecione o Gitmoji adequado de acordo com a intenção da alteração:

- `✨`: Nova funcionalidade ou recurso (`✨ Adicionando filtro de ordens de serviço por cliente`)
- `🐛`: Correção de bug (`🐛 Corrigindo recálculo de estoque ao cancelar ordem`)
- `🚨`: Resolução de code smells ou apontamentos SonarQube (`🚨 Resolvendo code smells apontados pelo SonarQube`)
- `🎨`: Ajuste visual, UI, acessibilidade ou formatação (`🎨 Corrigindo contraste WCAG AA no frontend`)
- `🔒`: Segurança, permissões ou credenciais (`🔒 Ajustando segurança Nginx para container sem root`)
- `🚚`: Movimentação ou renomeação estrutural (`🚚 Renomeando pasta do frontend para AutoReparos.Web`)
- `🔥`: Remoção de código, dependências ou arquivos legados (`🔥 Removendo código duplicado da raiz do repositório pai`)
- `📝`: Documentação ou especificações técnicas (`📝 Adicionando especificação técnica`)
- `👷`: Pipeline de CI/CD ou automação (`👷 Ajustando workflow de deploy no GitHub Actions`)
- `♻️`: Refatoração de arquitetura ou código (`♻️ Refatorando endpoints para minimal APIs`)
- `🧪`: Testes unitários ou de integração (`🧪 Adicionando testes de integração com Testcontainers`)

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
Formate a mensagem de commit seguindo estritamente o padrão:

```text
<gitmoji> <Descrição da ação em Português>

[corpo detalhado opcional]
```

#### Padrão de Referência do Repositório:
```text
📝 Adicionando especificação técnica
```

#### Exemplos Reais do Repositório:
- `📝 Adicionando especificação técnica`
- `✨ Adicionando suporte a CORS e envio de Role no Login`
- `🐛 Removendo modificador readonly do método applyMask na diretiva de máscara`
- `🔥 Removendo código duplicado da aplicação principal e lambda da raiz do repositório pai`
- `🔒 Ajustando segurança Nginx para rodar sem root na porta 8080`
- `🚨 Resolvendo apontamentos de code smell do SonarQube`
- `👷 Ajustando pipeline CI/CD para submódulos recursivos`

#### Comando de Execução do Commit:
```bash
git commit -m "📝 Adicionando especificação técnica"
```

---

### Step 6: Post-Commit State Verification
Confirm the commit succeeded and working tree is in a clean state:
```bash
git status
git log -1 --stat
```
