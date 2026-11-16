# TLC Spec Lean no AutoReparos

**Approved**: 2026-11-06 - user: "aprovado"

## Problem

As especificações atuais do AutoReparos vivem em formatos variados em `docs/specs`, sem um conjunto uniforme de obrigações verificáveis, vínculo obrigatório entre cada critério e sua prova ou etapa de verificação independente. Agentes e contribuidores podem iniciar a implementação a partir de planos descritivos sem congelar o que precisa ser verdadeiro.

Quando esta integração estiver pronta, novas features seguirão plano revisado, checks com provas, implementação e verificação independente, preservando a documentação histórica existente.

## Shape

Reutiliza `.agents/AGENTS.md` como instrução central e o pacote TLC instalado em `.agents/skills/tlc-spec-lean/`; as regras do workflow ficam nos artefatos `.specs/features/<feature>/` e são validadas pelos scripts locais.

1. contribuidores e agentes -> `AGENTS.md` (exists) -> encaminha às instruções do projeto
2. instruções -> `.agents/AGENTS.md` (exists) -> exige o fluxo TLC e a aprovação do plano
3. feature -> `.specs/features/<feature>/plan.md` (door 1) -> `.specs/features/<feature>/checks.md` (door 2)
4. checks -> `.agents/skills/tlc-spec-lean/scripts/` (door 3) -> validadores e provas
5. resultado -> `.specs/features/<feature>/proofs.md` e `.specs/features/<feature>/verification.md` (door 4)

```mermaid
flowchart LR
  A[AGENTS.md] --> B[.agents/AGENTS.md]
  B --> C[plan.md aprovado]
  C --> D[checks.md validado]
  D --> E[implementação]
  E --> F[.agents/skills/tlc-spec-lean/scripts/check_proofs.py (door 3)]\n  F --> G[verification.md (door 4)]
```

**Changes underneath**

| Front | What changes |
| --- | --- |
| agent guidance | novas features passam a exigir TLC Spec Lean e aprovação humana do plano |
| repository artifacts | pacote de skill e estrutura `.specs/` passam a ser a fonte para novas features; `docs/specs/` fica como histórico |

Data: none - this workflow stores only project specification artifacts, not business data.

## Surface

| Surface | In | Out | Status | Who |
| --- | --- | --- | --- | --- |
| gates locais | pasta da feature e seus artefatos | código de saída e mensagem do gate | 200 em validação aprovada, 400 em artefato inválido, 403 para chamadas sem acesso | contribuidores e agentes do repositório |
## Landing

| One-way door | Literal shape | Alternative rejected |
| --- | --- | --- |
| localização do pacote e artefatos | `.agents/skills/tlc-spec-lean/` e `.specs/features/<feature>/` | misturar com especificações históricas em `docs/specs/` |
| configuração do mutation count | `mutations: 3` em `.agents/AGENTS.md` e mesma contagem em cada `checks.md` | contagem implícita ou variável por agente |
| formato dos artefatos | nomes e estrutura definidos pelo skill TLC Spec Lean 2.1.0 | adaptar os validadores para convenções locais |

- Nada mais nesta integração é difícil de reverter.

## Criteria

### S1: Processo TLC nas features futuras (P1)

**Acceptance Criteria**

1. WHEN um contribuidor iniciar uma feature nova THEN as instruções do projeto SHALL exigir as fases TLC Spec Lean e aprovação explícita do plano, exceto mudanças pequenas sem porta de mão única conforme o skill.
2. WHEN uma feature iniciar THEN a configuração SHALL declarar `mutations: 3` e cada `checks.md` SHALL declarar a mesma quantidade.
3. WHEN o fluxo for configurado THEN os scripts e instruções TLC SHALL estar no repositório sob `.agents/skills/tlc-spec-lean/` com caminhos relativos portáveis.
4. IF um plano não tiver a linha de aprovação literal do usuário THEN `validate_checks.py` SHALL rejeitar o início da implementação daquela feature.
5. WHEN uma feature atingir verificação THEN as instruções SHALL exigir gates de provas e verificação TLC e o relatório SHALL destacar qualquer gate vermelho.
6. WHEN a integração for adicionada THEN os documentos e specs existentes em `docs/specs/` SHALL permanecer intactos.

## Out of scope

| Excluded | Why |
| --- | --- |
| Converter specs existentes em `docs/specs/` | migração histórica não é necessária para adoção futura |
| Implementar feature de negócio | fora do objetivo de integração do processo |
| Alterar workflows CI/CD | gates locais são o escopo inicial aprovado |

## Assumptions

| Assumption | Chosen default | Rationale | Confirmed? |
| --- | --- | --- | --- |
| Quantidade de mutações | 3 | valor padrão documentado pelo skill | n |
| Local dos artefatos | `.specs/` na raiz | localização padrão do skill | n |

**Open questions:** none - detalhes de instalação seguem o skill e convenções existentes.

## Sources

- `.agents/AGENTS.md` - instruções e convenções existentes do projeto
- Nenhuma captura ou referência visual fornecida.
