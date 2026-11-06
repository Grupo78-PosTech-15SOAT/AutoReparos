# Agent Instructions - AutoReparos

Consulte `.agents/AGENTS.md` para contexto de arquitetura, convenções de código e comandos do projeto. As regras abaixo complementam essas instruções.

## Spec-driven development (TLC Spec Lean)

Features novas neste repositório seguem o fluxo TLC Spec Lean. Use `.specs/STATE.md` e lições confirmadas como contexto; crie `.specs/features/<feature>/plan.md` com critérios EARS e aguarde aprovação explícita do usuário. Derive e valide `checks.md` antes da implementação; implemente contra esses checks congelados; execute os gates de provas e verificação antes de concluir. Para mudanças com menos de aproximadamente três arquivos e sem porta de mão única, use apenas `checks.md` com `## Intent`, conforme permitido pelo skill.

Configuração do projeto:

```markdown
## tlc-spec-lean
mutations: 3
```

Use Python 3.8+ e os scripts portáveis em `.specs/tlc-spec-lean/scripts/`. Não migre especificações históricas em `docs/specs/` salvo solicitação específica.
