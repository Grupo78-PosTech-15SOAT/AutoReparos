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

Fase 4: o registro único de decisões e o desenho transversal ficam em `docs/specs/fase4/index.md`; leia-o junto com `.specs/STATE.md` antes de planejar uma feature da fase.

## Monitoramento de Contexto e Handoff
- **Alerta de Contexto (60%)**: O assistente deve monitorar ativamente a densidade da conversa (volume de tokens consumidos, saídas de comandos e leituras extensas acumuladas).
- Sempre que a conversa se aproximar de ~60% da janela útil de contexto (ou após a conclusão de uma fase/feature extensa do SDD), o assistente **DEVE** exibir um aviso em destaque no rodapé da resposta:
  > ⚠️ **Alerta de Janela de Contexto (~60% atingido)**: A conversa está extensa. Para preservar a fidelidade das instruções, evitar esquecimento de contexto e manter o raciocínio afiado, atualize o `.specs/STATE.md` e inicie uma **nova conversa** (ou execute `/compact`).
- Antes de alternar de assunto ou encerrar uma sessão, o estado de trabalho e os próximos passos devem ser consolidados na seção `Handoff` de [.specs/STATE.md](.specs/STATE.md).

## Política de Uso e Delegação para Subagentes
Antes de planejar, pesquisar ou executar qualquer atividade, o assistente deve avaliar a conveniência de delegar tarefas para subagentes:
1. **Pesquisa e Diagnóstico Extenso (Automático)**:
   - Se a investigação exigir leitura de mais de 5 arquivos extensos, varredura profunda de logs ou exploração arquitetural ampla, deve acionar autonomamente o subagente `research` (read-only) para retornar apenas o diagnóstico consolidado e poupar a janela principal de contexto.
2. **Implementação de Tarefas e Execução**:
   - **Tarefas leves (≤ 5 tarefas atômicas ou ≤ 3 arquivos)**: Executar diretamente na conversa principal (*inline*) para evitar latência e sobrecarga de orquestração.
   - **Lotes médios/grandes (> 6 tarefas)**: Estruturar e despachar automaticamente subagentes de lote (`Batch Workers`, ~7 tarefas por lote) com workspaces isolados quando cabível, reportando na conversa principal apenas o sumário compacto das entregas.
3. **Verificação e Qualidade (Obrigatório e Automático)**:
   - Ao concluir as tarefas de uma funcionalidade, o assistente SEMPRE despacha automaticamente o subagente independente `Verifier` (`author != verifier`) para rodar os gates de teste, o sensor de discriminação (mutações) e gerar o relatório formal em `.specs/features/<feature>/validation.md`.

## Padrão de Commits
- Commits atômicos no formato Gitmoji + Conventional Commits em português, com descrição em minúsculas e sem ponto final:
  `<gitmoji> <descrição em minúsculas sem ponto final>`
  Exemplo: `✨ Adiciona fluxo de exportação de aniversariantes`
