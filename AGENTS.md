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

## Onde ficam as especificações e como preenchê-las

**Lugar correto.** A partir da Fase 4, toda especificação de uma fase fica em `.specs/features/fase<N>/` e em nenhum outro lugar. As Fases 1 a 3 continuam em `docs/specs/fase<N>/` como histórico: não crie arquivos novos lá.

| Conteúdo | Arquivo |
|---|---|
| Requisitos do PDF, estado atual, decisões e desenho transversal (o quê e por quê) | `.specs/features/fase<N>/especificacao/` — começa pelo `index.md` |
| Plano implementável de uma capacidade (como, verificável) | `.specs/features/fase<N>/<feature>/plan.md` → `checks.md` → provas e verificação |
| Plano e mapa da iniciativa | `.specs/features/fase<N>/plan.md`, `checks.md`, `FEATURES.md` |
| Decisões para agentes e handoff | `.specs/STATE.md` (`AD-xxx`, sempre citando o `D-xx` do índice) |

**Protocolo** (detalhes no § 8 de `.specs/features/fase4/especificacao/index.md`):

1. Antes de planejar ou implementar algo da fase, leia `especificacao/index.md` (§ 6 decisões, § 7 cronograma) e `.specs/STATE.md`.
2. Decisão nova: registre primeiro no § 6 do índice (`D-xx`, data e as palavras de quem decidiu; abertas no § 6.3 com recomendação); se afetar a implementação, crie o `AD-xxx` no `STATE.md` citando o `D-xx`; atualize a spec transversal afetada; mudança de decisão anterior vai também para o § 6.4.
3. Plano aprovado afetado por decisão: revise-o e troque `**Approved**` por `**Revisão pendente de aprovação**`. Nunca escreva a linha `**Approved**` sem as palavras do usuário; os `checks.md` são derivados de novo depois da aprovação.
4. Spec transversal nova: siga as seções das existentes (cabeçalho; Critério; Estado atual; Lacunas; Desenho; Passos para a mudança; Critérios de aceite) e registre-a no mapa (§ 3) e na matriz (§ 4) do índice.
5. Feature nova: `<feature>/plan.md` no formato TLC, com as decisões de origem no topo e a seção da spec transversal em "Sources", em vez de copiar o desenho; registre no `FEATURES.md` e no § 3.1 do índice.
6. Antes de commitar: `python3 scripts/specs/verificar_links.py` e `python3 .specs/tlc-spec-lean/scripts/validate_plan.py fase<N>/<feature>` sem erros; diagramas Mermaid válidos; caminhos relativos; português; nomes de serviços, filas e eventos em português.

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
