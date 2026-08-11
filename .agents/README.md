# .agents

Project-local standard for AGENTS.md + MCP + SKILLS.

## Quick workflow
- `agents status` to inspect enabled integrations and MCP state.
- `agents mcp add <url-or-name>` to add one server for all selected tools.
- `agents mcp test --runtime` to validate connectivity.
- `agents sync` to materialize generated configuration.
- `agents sync --check` for CI-safe drift detection.

## Knowledge & Specs
- `knowledge/`: Architectural knowledge base for AI agents
  - `knowledge/domain-model.md`: Domain entities, value objects, state machines, invariants
  - `knowledge/architecture-overview.md`: Clean Architecture layers, DI setup, Minimal API + Controller pattern, OpenTelemetry
  - `knowledge/database-schema.md`: PostgreSQL schema, EF Core mappings, migration history, seed pipeline
- `specs/`: Implementation specifications for AI agents
  - `specs/api-contracts.md`: REST endpoints, request/response DTOs, authentication, ProblemDetails
  - `specs/branching-strategy.md`: Git Flow branching model, branch taxonomy (feat/, fix/, refactor/), lifecycle, PR base rules, and EKS deploy triggers
  - `specs/vertical-slice-pattern.md`: Feature implementation walkthrough across Domain, Application, Infra, API, and Web


## Root instruction file
- `../AGENTS.md`: canonical instruction document

## Local/private files (do not commit)
- `local.json`: machine-specific MCP overrides and secrets

## Generated files
- `generated/*`: renderer outputs used by `agents sync`
- `mcp_config.json`: Antigravity CLI workspace MCP config
- `generated/vscode.settings.state.json`: managed VS Code hide state

## Common materialized outputs
- `.codex/config.toml`
- `.gemini/settings.json`
- `.vscode/mcp.json`
- `.vscode/settings.json`
- `.cursor/mcp.json`
- `.agents/mcp_config.json`
- `opencode.json`
- `.windsurf/skills/`
- `~/.codeium/windsurf/mcp_config.json` (global)
