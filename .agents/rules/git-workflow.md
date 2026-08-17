# Git Workflow & Conventional Commits Guide

This rule specifies the branch naming strategies, commit message formatting, Pull Request (PR) standards, and pre-merge verification rules for `AutoReparos`.

---

## 1. Branch Strategy & Naming Conventions

The repository follows Git Flow conventions with `develop` as the primary integration branch and `main` for production releases and automated AWS EKS deployments.

All feature/fix branches must be lowercase, use hyphens as word separators, and be prefixed according to their purpose:

| Prefix | Target Branch | Usage | Example |
| :--- | :--- | :--- | :--- |
| `feat/` | `develop` | New functionality or feature enhancement | `feat/monorepo-frontend` |
| `fix/` | `develop` | Fixing a bug or code smell | `fix/calculo-estoque-insumo` |
| `refactor/` | `develop` | Architectural or code structure improvement | `refactor/clean-arch-adjustments` |
| `docs/` | `develop` / `main` | Documentation updates | `docs/add-tech-challenge-docs` |
| `chore/` | `develop` | Tooling, dependencies, or CI/CD updates | `chore/update-dependencies` |
| `hotfix/` | `main` & `develop` | Urgent production fix | `hotfix/jwt-auth-header-leak` |

---

## 2. Commit Message Conventions (Gitmoji + Conventional Commits)

Commit messages in `AutoReparos` follow Conventional Commits combined with Gitmoji prefixes in imperative Portuguese or English:

```text
<gitmoji> <type>(<scope>): <short imperative summary>

[optional detailed body]
```

### Gitmoji & Type Mappings

| Gitmoji | Type | Usage | Example |
| :--- | :--- | :--- | :--- |
| `✨` (`:sparkles:`) | `feat` | New feature | `✨ Suporte a CORS e envio de Role no Login` |
| `🐛` | `fix` | Bug fix | `🐛 Removendo modificador readonly na mascara` |
| `🚨` | `fix` / `test` | Fixing SonarQube code smells or broken tests | `🚨 Resolução de apontamentos do SonarQube` |
| `🎨` | `style` / `refactor` | Code formatting, UI contrast, or refactoring | `🎨 Correção de contraste WCAG AA` |
| `🔒` | `security` | Security adjustments or hardening | `🔒 Ajuste de seguranca Nginx (USER nginx)` |
| `🚚` | `refactor` / `chore` | File renaming, folder moving, structural changes | `🚚 Renomeando pasta do frontend para AutoReparos.Web` |
| `📝` | `docs` | Documentation updates | `📝 Adicionando secao de Arquitetura Frontend` |
| `👷` / `💚` | `ci` / `build` | CI/CD pipeline changes (`.github/workflows/deploy.yml`) | `👷 Ajuste no job de build do GitHub Actions` |

---

## 3. GitHub Actions CI/CD Integration

The repository runs an automated 5-stage pipeline on push/PR to `main` and `develop` ([`deploy.yml`](../../.github/workflows/deploy.yml)):

1. **Compilação**: Restores dependencies and builds `AutoReparos.slnx` on .NET 10.
2. **Testes**: Runs `AutoReparos.Domain.Tests`, `AutoReparos.Application.Tests`, and `AutoReparos.IntegrationTests`.
3. **Infraestrutura**: (Triggers on `push` to `main`) Runs `terraform init`, `validate`, `apply`, logs in to AWS ECR, and pushes Docker image.
4. **Deploy**: Upgrades Kubernetes EKS deployment using Helm (`helm upgrade --install autoreparos ./k8s`).
5. **Auto-Destroy**: Automatically cleans up AWS demo environment after 15 minutes.

### Examples

```text
feat(clientes): add pagination support for client listing endpoint

fix(ordem-servico): correct total price recalculation on service removal

refactor(auth): simplify JWT payload extraction in HttpContext extension

docs(readme): add docker setup instructions for local development
```

---

## 3. Commit Best Practices

1. **Atomic Commits**: Keep commits small, focused, and self-contained. Do not combine unrelated changes in a single commit.
2. **Imperative Mood**: Write commit titles in the imperative mood ("add feature" not "added feature" or "adds feature").
3. **No Secrets**: Never commit `.env` files, JWT secrets, passwords, or cloud credentials.

---

## 4. Pull Request (PR) Conventions

### PR Title Format
PR titles must follow Conventional Commits formatting matching the primary branch work (e.g. `feat(clientes): implement client creation use case`).

### PR Description Template

```markdown
## Summary
Brief description of the changes introduced by this PR.

## Motivation & Context
Why is this change required? What problem does it solve? (Link to issue/ticket if applicable)

## Type of Change
- [ ] Feature
- [ ] Bug Fix
- [ ] Refactoring
- [ ] Breaking Change
- [ ] Documentation

## Verification & Testing
How was this change tested?
- [ ] Unit Tests (`dotnet test`, `yarn test`)
- [ ] Integration Tests (`AutoReparos.IntegrationTests`)
- [ ] Manual Verification / API Smoke Test

## Pre-Merge Checklist
- [ ] Code builds without errors or warnings.
- [ ] All new and existing tests pass cleanly.
- [ ] Code follows project architecture (`.agents/rules/`).
- [ ] Sensitive secrets or connection strings are not committed.
```

---

## 5. Pre-Merge Verification Checklist for Developers

Before requesting a PR review, developers/AI agents must verify:

1. **Solution Compilation**: `dotnet build AutoReparos.slnx` succeeds with 0 errors and 0 warnings.
2. **Backend Tests**: `dotnet test AutoReparos.slnx` succeeds.
3. **Frontend Tests**: `yarn test` / `ng test` in `AutoReparos.Web` completes successfully.
4. **Lint & Formatting**: Code formatting adheres to `.editorconfig` and Prettier.
