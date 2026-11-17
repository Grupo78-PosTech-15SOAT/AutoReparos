# .agents

Padrão local do projeto para instruções de agentes de IA (`AGENTS.md`), servidores MCP e skills. Uma única fonte alimenta Claude Code, Codex, Gemini, Copilot, Cursor, Windsurf, Junie, OpenCode e Antigravity.

## Fluxo rápido (CLI `agents`)

| Comando | Para que serve |
| --- | --- |
| `agents status` | Mostra as integrações habilitadas e o estado dos MCP |
| `agents mcp add <url-ou-nome>` | Adiciona um servidor MCP para todas as ferramentas selecionadas |
| `agents mcp test --runtime` | Valida a conectividade dos MCP |
| `agents sync` | Gera os arquivos de configuração de cada ferramenta |
| `agents sync --check` | Detecta divergência sem alterar nada (seguro para CI) |

## Instruções

| Arquivo | Conteúdo |
| --- | --- |
| [`../AGENTS.md`](../AGENTS.md) | Instruções canônicas do repositório: fluxo TLC Spec Lean, onde ficam as especificações, delegação para subagentes e padrão de commits |
| [`AGENTS.md`](./AGENTS.md) | Contexto de arquitetura, convenções de código e comandos do projeto |
| [`GEMINI.md`](./GEMINI.md) | Instruções equivalentes para o Gemini |
| [`agents.json`](./agents.json) | Configuração da CLI `agents` (ferramentas e MCP habilitados) |

As especificações da Fase 4 ficam em [`../.specs/features/fase4/`](../.specs/features/fase4/) e o estado e as decisões para agentes em [`../.specs/STATE.md`](../.specs/STATE.md).

## Base de conhecimento (`knowledge/`)

| Arquivo | Conteúdo |
| --- | --- |
| [`domain-model.md`](./knowledge/domain-model.md) | Entidades, value objects, máquinas de estado e invariantes |
| [`architecture-overview.md`](./knowledge/architecture-overview.md) | Camadas da Clean Architecture, injeção de dependência, Minimal APIs e OpenTelemetry |
| [`database-schema.md`](./knowledge/database-schema.md) | Esquema PostgreSQL, mapeamentos EF Core, migrations e seed |
| [`api-contracts.md`](./knowledge/api-contracts.md) | Endpoints REST, DTOs, autenticação e ProblemDetails |
| [`observability-stack.md`](./knowledge/observability-stack.md) | Stack de observabilidade (OTel Collector, Prometheus, Jaeger, Loki, Grafana) |

## Regras (`rules/`)

| Arquivo | Conteúdo |
| --- | --- |
| [`branching-strategy.md`](./rules/branching-strategy.md) | Git Flow, taxonomia de branches, base dos PRs e gatilhos de deploy |
| [`git-workflow.md`](./rules/git-workflow.md) | Fluxo de commits e Pull Requests |
| [`language-and-writing-standards.md`](./rules/language-and-writing-standards.md) | Idioma e estilo de escrita |
| [`clean-arch-ddd.md`](./rules/clean-arch-ddd.md) | Invariantes de Clean Architecture e DDD |
| [`vertical-slice-pattern.md`](./rules/vertical-slice-pattern.md) | Implementação de uma feature de ponta a ponta |
| [`csharp-dotnet.md`](./rules/csharp-dotnet.md) | Convenções de C# e .NET |
| [`postgres-efcore.md`](./rules/postgres-efcore.md) | Convenções de PostgreSQL e EF Core |
| [`angular-frontend.md`](./rules/angular-frontend.md) | Convenções do frontend Angular |
| [`observability.md`](./rules/observability.md) | Regras de métricas, traces e logs |
| [`code-review.md`](./rules/code-review.md) | Critérios de revisão de código |
| [`submodules-ci-and-docs-integrity.md`](./rules/submodules-ci-and-docs-integrity.md) | Submódulos, CI/CD e integridade da documentação |

## Skills (`skills/`)

Lista completa e convenções em [`skills/README.md`](./skills/README.md).

## Arquivos locais (não versionar)

- `local.json`: sobrescritas de MCP e segredos da máquina.
- `mcp_config.json`: configuração MCP do workspace do Antigravity.

## Arquivos gerados

- `generated/*`: saídas dos renderizadores usadas pelo `agents sync` (uma por ferramenta).
- `generated/vscode.settings.state.json`: estado de ocultação gerenciado no VS Code.

Saídas materializadas mais comuns:

- `.codex/config.toml`
- `.gemini/settings.json`
- `.vscode/mcp.json` e `.vscode/settings.json`
- `.cursor/mcp.json`
- `.agents/mcp_config.json`
- `opencode.json`
- `.windsurf/skills/`
- `~/.codeium/windsurf/mcp_config.json` (global)
