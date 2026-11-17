# Skills

Skills do projeto usadas pelas ferramentas de IA suportadas. Cada skill vive na própria pasta, com um `SKILL.md`.

## Skills disponíveis

| Skill | Quando usar |
| --- | --- |
| [`tlc-spec-lean`](./tlc-spec-lean/SKILL.md) | Fluxo spec-driven das features: `plan.md` com critérios EARS, `checks.md` congelado, provas e verificação (scripts em `tlc-spec-lean/scripts/`) |
| [`csharp-clean-arch`](./csharp-clean-arch/SKILL.md) | Casos de uso, entidades, value objects, repositórios e testes xUnit em .NET 10 com Clean Architecture e DDD |
| [`postgres-efcore`](./postgres-efcore/SKILL.md) | Migrations EF Core 10, mapeamentos `IEntityTypeConfiguration`, índices e diagnóstico de PostgreSQL |
| [`angular-component`](./angular-component/SKILL.md) | Componentes standalone, serviços, formulários e testes Vitest no AutoReparos.Web |
| [`observability-otel`](./observability-otel/SKILL.md) | Métricas e traces com OpenTelemetry, Jaeger, Prometheus, Grafana, Loki e New Relic |
| [`code-review`](./code-review/SKILL.md) | Checklist de revisão, aderência à arquitetura e qualidade dos testes |
| [`git-review`](./git-review/SKILL.md) | Revisão de diffs e PRs, auditoria de segurança e detecção de quebras de contrato |
| [`git-commit`](./git-commit/SKILL.md) | Mensagens Gitmoji + Conventional Commits, validação antes do commit e proteção de segredos |
| [`git-pr`](./git-pr/SKILL.md) | Preparar e abrir Pull Requests com `gh pr create` e o template do projeto |
| [`docs-research`](./docs-research/SKILL.md) | Reunir documentação oficial atualizada e transformá-la em checklist de implementação |
| [`mcp-troubleshooting`](./mcp-troubleshooting/SKILL.md) | Diagnosticar configuração e execução de servidores MCP com a CLI `agents` |
| [`skill-guide`](./skill-guide/SKILL.md) | Criar, organizar e manter skills nesta pasta |

## Estrutura

```text
.agents/skills/<skill-id>/SKILL.md
```

O arquivo [`../.skill-lock.json`](../.skill-lock.json) registra as versões das skills instaladas.

## Modelo mínimo de `SKILL.md`

```md
---
name: minha-skill
description: Quando esta skill deve ser usada.
---

Instruções passo a passo para o agente.
```

## Convenções

- O nome da pasta é igual ao `name` do frontmatter.
- `name` em kebab-case (letras minúsculas, números e `-`).
- `description` específica, para que as ferramentas acionem a skill no momento certo.
- Skills estreitas e reutilizáveis: um resultado claro por skill.
