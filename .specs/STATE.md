# Project state

## Decisions

| ID | Decision | Rationale | Status | Date |
| --- | --- | --- | --- | --- |
| AD-001 | Integrações transitórias usam limites padrão de retry por dependência, backoff exponencial com jitter e DLQ/alerta ao esgotar; cobrança capturada nunca é repetida | User: "Adote limites padrão por dependência, backoff exponencial com jitter e DLQ/alerta ao esgotar; nunca repetir cobrança capturada." Evita retries ilimitados e cobranças duplicadas | active | 2026-11-06 |
| AD-002 | Commits saem só no nome do usuário, sem `Co-Authored-By` nem outra atribuição ao Claude ou outro agente de IA; mensagem no formato `<gitmoji> <Descrição>` com a primeira letra depois do gitmoji em maiúscula, sem ponto final e sem prefixo `feat(...)` | User: "Preciso que não apareça que você commitou, somente eu" e "deixe a primeira letra maiúscula sempre depois do gitmoji". Prevalece sobre o texto "minúsculas" de `AGENTS.md` (o exemplo do próprio arquivo já usa maiúscula) | active | 2026-11-09 |
| AD-003 | Cada feature só fecha com Verifier independente (sub-agente novo, `author != verifier`); veredito PASS exige nenhum achado ALTA ou MÉDIA aberto, achados BAIXA e riscos conscientes ficam em `## Open items` | Rodadas 1 e 2 sempre acharam coisas novas; sem um critério de parada o ciclo não termina. Critério definido pela autora do trabalho e aplicado pelo Verifier | active | 2026-11-09 |
| AD-004 | os-service: achados 10 (histórico além da criação) e 12 (filtros truncados em `proofs.md`) declarados fora de escopo; transições de status ficam com `execucao` e `saga-mensageria` | User: "Declarar fora de escopo". Registrado em `## Out of scope` do plano do os-service | active | 2026-11-09 |
| AD-005 | Controle otimista de concorrência por `xmin` (coluna de sistema) mais coluna `Revisao` nos agregados com filhos; conflito retorna 409 com código estável | User: "Implementar e testar". Os checks afirmavam concorrência sem teste nem token | active | 2026-11-09 |
| AD-006 | `PATCH` com campo ausente não altera a observação; `null` limpa | User: "Campo ausente não altera" | active | 2026-11-09 |
| AD-007 | Billing: reembolso reserva um registro `Pendente` (conta no limite), confirma após o provedor aceitar e só desfaz em falha definitiva 4xx; desfecho incerto mantém `Pendente` e retoma pela mesma referência | User: "Corrigir os 3" achados N1 a N3 da rodada 2. Evita reembolso em dobro | active | 2026-11-09 |
| AD-008 | Execution Service: papéis `Mecanico`/`Administrador` operam fila, etapas, conclusão e falha; só `Administrador` reexecuta; entrada pelo evento SQS `ordem-servico.liberada.v1` (deduplicação por `eventId` e por OS); saída `execucao.concluida.v1` (OS Service e Billing) e `execucao.falhou.v1` (OS Service) via outbox. Endpoints extras `.../conclusao` e `.../falha` | Decisões do builder, não confirmadas ponto a ponto pelo usuário; documentadas no README do serviço. Revisar na feature `saga-mensageria` | active | 2026-11-09 |
| AD-009 | `Authentication:RequireHttpsMetadata` configurável no OS Service (padrão `true`); só a execução de prova local usa `false` | Necessário para o emissor JWT local de teste da prova C5. Escolha do usuário: "Adicionar toggle de config" | active | 2026-11-09 |
| AD-010 | Nomes de repositórios, serviços, filas e eventos em português, sem misturar idiomas (regra L-02): `AutoReparos.OS`, `AutoReparos.Cobranca` (hoje `Billing`), `AutoReparos.Execucao` (hoje `Execution`), `AutoReparos.Cadastros` (hoje `App`). Sufixos técnicos (`Api`, `Infrastructure`, `Tests`) seguem `.agents/AGENTS.md` | José: "não consigo concordar com os nomes sendo inglês e português" e "Português". Ver `.specs/features/fase4/especificacao/index.md` D-02 e L-02. Renomeação pendente | active | 2026-11-09 |
| AD-011 | Saga além do mínimo (D-18): reserva de insumos em duas fases (reservar, confirmar na conclusão, cancelar na compensação), travas de estado (409 `saga_em_andamento` e escrita condicional), prazos técnicos por passo, lembretes sem expiração, console de administração de Sagas e reprocessamento de DLQ. Pré-autorização do pagamento fica fora | José: "Vamos então fazer o a, c e d". Desenho em `.specs/features/fase4/especificacao/saga/fase4_spec_saga_pattern.md` § 4.5–4.7 | active | 2026-11-09 |
| AD-012 | Regra L-01: cada passo grava estado de negócio, estado da Saga, Outbox e Inbox numa única transação de um único banco. No OS Service vale a opção C: Saga no DynamoDB, OS no PostgreSQL, nenhum handler grava nos dois; status e histórico da OS são aplicados ao consumir `saga.avancou.v1`; Outbox do DynamoDB publicada por Streams | José: "a duplicidade atômica como transação do saga e postgres deve ser lei". Mantém a decisão OS + DynamoDB + SQS. Ver `.specs/features/fase4/especificacao/saga/fase4_spec_saga_pattern.md` § 4.8 | active | 2026-11-09 |
| AD-013 | Fronteiras (D-01): 3 serviços de negócio (OS, Cobrança, Execução) + Cadastros. Catálogo de serviços e insumos com reserva ficam na Execução; o App vira Cadastros (clientes, veículos, usuários, emissão de JWT/JWKS, notificações por eventos); o DynamoDB é o estado da Saga (D-04); não há serviço de Estoque | José aceitou a recomendação de 09/11 (só discordou dos nomes e da atomicidade, tratados em AD-010 e AD-012). Ver `.specs/features/fase4/especificacao/microsservicos/fase4_spec_separacao_microsservicos.md` | active | 2026-11-09 |
| AD-014 | Fan-out por SNS (D-19): um tópico SNS por serviço produtor e uma fila SQS por consumidor assinando o tópico, com filtro por `tipo`; comandos seguem direto para a fila do participante. Substitui a "uma linha de Outbox por destino" do Execution | José: "Recomendado" (10/11) | active | 2026-11-10 |
| AD-015 | Falta de saldo na reserva (D-20): nada é reservado; a Saga vai para `AguardandoInsumos` com alerta diário; retentar ou cancelar por pessoa | José: "Ok, pode ser" (10/11) | active | 2026-11-10 |
| AD-016 | Nome `AutoReparos.Cobranca` confirmado para o Billing (D-02). Deploy de cada serviço pelo próprio repositório, pai só com a plataforma (D-13); ambientes local, staging na `develop` e produção na `main` com aprovação (D-22). O nome do App foi decidido em AD-017 | José: "Cobrança" (10/11); D-13 e D-22 convergem com `infra-ci-observabilidade` e com o plano pai | active | 2026-11-10 |
| AD-017 | O App reduzido passa a se chamar `AutoReparos.Cadastros` (D-23) | José: "Pode manter: AutoReparos.Cadastros" (10/11) | active | 2026-11-10 |
| AD-018 | Especificações da Fase 4 em diante ficam só em `.specs/features/fase<N>/`: `especificacao/` para requisitos, estado atual, decisões e desenho; uma pasta TLC por feature. `docs/specs/fase1-3` fica como histórico, sem arquivos novos. Protocolo de preenchimento no `AGENTS.md` e no § 8 do índice; links verificados por `scripts/specs/verificar_links.py` | José: "vamos mover nosso conteúdo para .specs com o Enrico" (10/11) | active | 2026-11-10 |

| AD-019 | Saga orquestrada no OS Service com estado em DynamoDB (`autoreparos-os-saga`); nasce em `orcamento.aprovado.v1` (evita escrita dupla); transição e comandos pendentes gravados na mesma transação antes de publicar; compensações em ordem inversa só para orçamento aprovado e execução liberada; execução concluída e pagamento capturado só têm recuperação para frente; cancelamento entra por `ordem-servico.cancelar.v1` (sem rota REST nova, Surface vazia). Contrato em `saga-mensageria/contracts.md` | Decisões de projeto da autora para fechar o contrato entre OS, Billing e Execução; o plano aprovado não foi alterado. User: "pode seguir com o saga-mensageria". Revisar se discordar | active | 2026-11-10 |
| AD-020 | Surface da Saga ampliada com `POST /api/v1/ordens-servico/{id}/cancelamento` (Administrador ou Atendente; 202, 401, 403, 404, 409, 422) como gatilho de produção da compensação; status da OS após a execução é `Concluida` (o Billing só libera a cobrança com esse valor); Execution cancela também em `Falhou` | User: "Adicionar rota de cancelamento" (2026-11-10) após o Verifier apontar compensação sem gatilho. O nome `Concluida` corrige erro do contrato inicial (ALTA do Verifier). Registrado em `plan.md` (Surface) e `contracts.md` | active | 2026-11-10 |
| AD-021 | D-18 (reserva de insumos em duas fases, travas de estado, prazos, console admin e reprocessamento de DLQ) aceita como feature separada `saga-compensacoes`, com `insumos`; o plano e os checks de `saga-mensageria` (PASS) ficam congelados e a evolução não reabre esse veredito | User: "aceitar d-18 como recomendado" | active | 2026-11-10 |
| AD-022 | D-24: aplicar L-02 aos tipos de mensagem já implementados na feature `eventos-portugues`: `billing.orcamento.cancelar.v1` passa a `cobranca.cancelar-orcamento.v1`; os demais tipos existentes permanecem. O plano aprovado e os checks de `saga-mensageria` continuam congelados (AD-021). A estratégia de compatibilidade é proposta no novo plano, ainda pendente de aprovação | User: "Vamos trocar os eventos para português" (10/11); o nome alvo já consta no catálogo da spec transversal | active | 2026-11-10 |
## Unificação da especificação da Fase 4 - 2026-11-09 (atualizada em 10/11)

- **Entrega do trabalho: 17/11/2026.** Divisão em 5 tarefas e cronograma em `.specs/features/fase4/especificacao/index.md` § 7.1 e § 7.2.
- Planos aguardando aprovação do usuário (todos sem `**Approved**`; os `checks.md` antigos de `infra-ci-observabilidade` e `pacote-entrega` devem ser derivados de novo): `infra-ci-observabilidade`, `pacote-entrega` (revisados) e `insumos`, `cadastros`, `saga-compensacoes` (novos). `saga-mensageria` mantém o plano aprovado e o PASS (AD-021).
- Em 10/11 a especificação do José saiu de `docs/specs/fase4/` e passou a morar em `.specs/features/fase4/especificacao/` (o quê e por quê), ao lado das pastas TLC de cada feature (como, verificável). O `index.md` dessa pasta é o registro único de decisões (D-xx); cada `AD-xxx` daqui cita o seu D-xx. Papéis no § 3.1 e protocolo de preenchimento no § 8 do índice e no `AGENTS.md` da raiz.
- Na mesclagem local de `feature/os-service-contracts` em `docs/fase4-especificacao-inicial`, os ponteiros de App, AuthLambda, Infra.Database e Infra.K8s ficaram na `develop` atual. A branch `feature/os-service-contracts` do Infra.K8s partiu de `607f1de`, sem o provisionamento centralizado: rebasear na `develop` antes do PR.
- Decisões em aberto (nenhuma bloqueia `saga-mensageria`): D-11, D-21, D-12, D-14b, D-15, D-16, D-17.

## Handoff (retomada em outro computador) - 2026-11-09

### Onde estamos

Iniciativa **Fase 4** em `.specs/features/fase4/` (mapa em `FEATURES.md`). Branch de trabalho do repositório principal: `feature/os-service-contracts` (enviada ao GitHub). Todos os repositórios estavam com árvore limpa e `HEAD` local igual ao remoto ao fechar este handoff, exceto este próprio commit.

| Feature | Estado | Evidência |
| --- | --- | --- |
| `os-service` | **PASS** (3ª rodada de Verifier). Provas 7/7, 3 mutações mortas | `os-service/verification.md`, `proofs.md` |
| `billing` | **PASS** (3ª rodada). Provas 11/11, 3 mutações mortas | `billing/verification.md`, `proofs.md` |
| `execucao` | **PASS** (1ª rodada). Provas 8/8 (6 checks, build, format), 3 mutações mortas | `execucao/verification.md`, `proofs.md` |
| `saga-mensageria` | **PASS** (2ª rodada de Verifier). Provas 9/9, 3 mutações mortas; regressão no mesmo HEAD: os-service 7/7, billing 11/11, execucao 8/8, 3 mutações mortas cada | `saga-mensageria/verification.md`, `proofs.md` |
| `eventos-portugues` | **PASS** (1ª rodada de Verifier). Provas 13/13, 3 mutações mortas (OS `45defd3`, Cobrança `8e7bff7`, root `e078c04`) | `eventos-portugues/verification.md`, `proofs.md` |
| `infra-ci-observabilidade` | só `plan.md` e `checks.md`; build não iniciado | depende dos serviços |
| `pacote-entrega` | só `plan.md` e `checks.md`; build não iniciado | inclui vídeo e PDF final, que o agente não produz sozinho |

Itens abertos (todos de gravidade baixa, aceitos) estão em `## Open items` de cada `verification.md`. Os que mais importam:
- **Contrato de eventos entre serviços não fecha.** O OS só publica `ordem-servico.criada.v1` e `atualizada.v1`; nunca `liberada.v1`. Nem OS nem Billing consomem `execucao.concluida.v1` ou `execucao.falhou.v1`. É trabalho da `saga-mensageria`.
- Billing: checkout concorrente chama o Mercado Pago duas vezes (mesma chave); webhook de tentativa antiga pode ser aplicado à tentativa nova (todas compartilham `external_reference`); sem varredura de reservas de reembolso `Pendente`; status 409/502/503 fora da tabela Surface do plano.
- os-service: achado 11 parcial (falta teste do script SQL dos papéis e do lado "via eventos" do AC 5); mensagens antigas do outbox em PascalCase; PATCH valida o corpo antes de procurar a OS (422 antes de 404).
- Execution: alertas são só log `Critical`; sem paginação na fila; sem purga de `eventos_processados` e `mensagens_saida`; falha antiga continua exposta após a reexecução; backoff do outbox sem teste de crescimento e teto.

### Repositórios e branches

O repositório principal usa submódulos fixados por commit. Em outro computador: `git clone --recurse-submodules <url do principal>` e `git checkout feature/os-service-contracts`, depois `git submodule update --init --recursive`.

| Submódulo | Branch | Remoto |
| --- | --- | --- |
| `AutoReparos.OS` | `feature/os-service-contracts` | `Grupo78-PosTech-15SOAT/AutoReparos.OS` |
| `AutoReparos.Billing` | `feature/os-service-contracts` | `.../AutoReparos.Billing` |
| `AutoReparos.Execution` | `feature/execution-service` | `.../AutoReparos.Execution` |
| `AutoReparos.Infra.K8s` | `feature/os-service-contracts` (telemetria OTLP no chart) | `.../AutoReparos.Infra.K8s` |
| `AutoReparos.App`, `AuthLambda`, `Infra.Database` | `develop` (sem mudanças nossas) | `.../AutoReparos.App` etc. |

- Os remotos de OS, Billing e Execution só têm a branch de feature, **sem `main`**. Antes do primeiro PR é preciso criar a `main` ou definir outra base. Nenhum PR foi aberto.
- O Billing deixou de ser pasta rastreada no principal e virou submódulo (histórico extraído com `git subtree split`).
- Branches e commits foram reescritos duas vezes com `--force-with-lease` para remover o trailer do Claude (AD-002). Se algum clone antigo existir, refaça-o.

### O que NÃO vem no repositório (recrie no outro computador)

1. **`CLAUDE.md` é ignorado pelo `.gitignore` (linha 406).** Crie na raiz um arquivo com uma única linha: `@AGENTS.md`.
2. **Preferências da memória do Claude não sincronizam.** As regras de commit estão em AD-002; repita-as ao Claude.
3. **Skill `tlc-spec-lean`:** instalada no repositório em `.agents/skills/tlc-spec-lean/` (SKILL.md, `references/` e `scripts/`) para descoberta automática no Antigravity e Claude Code. Os scripts do gate ficam em `.agents/skills/tlc-spec-lean/scripts/` e rodam com Python 3.8+.
4. **Ferramentas:** Git Bash, Docker (imagem `postgres:16-alpine`; os testes de Billing e Execution usam Testcontainers), .NET 10 SDK com acesso ao NuGet, `dotnet-ef` (`dotnet tool install --global dotnet-ef`), Python com `pip install pyjwt cryptography`.
5. **Ambiente de prova do os-service (C5):** agora versionado em `.specs/features/fase4/os-service/proof-env/` (`harness.sh`, `run_proofs.sh`, `mock_issuer.py`, `README.md`). Comando: `bash .specs/features/fase4/os-service/proof-env/run_proofs.sh --mutations .specs/features/fase4/os-service/mutations.json`. Os containers e processos locais (`os-proof-pg`, emissor na porta 8099, API na 5083) não migram.

### Como rodar o gate de uma feature

1. Árvore git **limpa** (o `check_proofs.py` registra `Tree: dirty` e sai com 1 caso contrário).
2. `python .agents/skills/tlc-spec-lean/scripts/check_proofs.py fase4/<feature> --mutations .specs/features/fase4/<feature>/mutations.json` (OS precisa do `run_proofs.sh` acima; Billing e Execution rodam direto).
3. O Verifier independente propõe as 3 mutações em `mutations.json`; o comando `proof` de cada uma deve começar com `dotnet build ... --no-restore -v q &&`, senão o binário antigo sobrevive e a mutação parece não morta.
4. Não rode dois `check_proofs.py` ao mesmo tempo (mutações do mesmo arquivo colidem) e não edite código enquanto uma mutação roda.
5. Limitação conhecida: `validate_verification.py` exige que `proofs.md` esteja no `HEAD` exato, e commitar o próprio `proofs.md` move o `HEAD`. O erro "proofs.md was written at X and HEAD is Y" reaparece depois de cada commit; não é defeito do código.
6. A célula de tabela do `verification.md` não pode conter `|` (nem `\|`): o validador conta como coluna.
7. Cada achado fechado leva o hash do commit de correção na coluna `Closed`. Achados BAIXA ficam em `## Open items`, sem hash.

### Saga-mensageria: concluída (2026-11-10)

- **Veredito: PASS** na 2ª rodada de Verifier independente (AD-003): nenhum achado ALTA ou MÉDIA aberto; as 4 ALTA/MÉDIA da 1ª rodada e 6 BAIXAS estão fechadas com hash no `verification.md`; as BAIXAS restantes e os riscos conscientes estão em `## Open items`.
- **HEADs verificados**: OS `d8e8503` (orçamento aprovado após cancelamento compensa sem reativar a OS), Billing `a0f4870`, Execution `fb56593`. Gate no HEAD `75b086b` do principal, árvore limpa: saga 9/9, os-service 7/7, billing 11/11, execucao 8/8, **3 mutações mortas em cada**.
- **Riscos abertos (BAIXA)**: corrida de milissegundos entre `orcamento.aprovado.v1` e a rota de cancelamento (N-1); literais de contrato duplicados entre OS e Billing (N-2); `Registros` da Saga sem limite; Billing sem `correlacaoId` em `pagamento.resultado.v1`; alertas só em log; mensagens fora de ordem rejeitadas nunca reavaliadas.
- **Gate no Windows**: o `mutate.py` da skill (agora em `.agents/skills/tlc-spec-lean/scripts/`) trata `SIGHUP` ausente via `getattr`, então o contorno com `sitecustomize.py` deixou de ser necessário (o selftest da skill passa: 79 mortas, 0 sobreviventes). Mesmo assim, confira sempre que a linha `Result` mostre `mutations 3 killed`; `mutations 0 killed, 0 not` significa que as mutações NÃO rodaram.
- **Migração da skill**: concluída e commitada em `a2d8354` (skill em `.agents/skills/tlc-spec-lean/`, referências atualizadas em `AGENTS.md`, `.agents/AGENTS.md` e `run_proofs.sh`). Os stashes `wip-user-agents-paths*` foram descartados.

### Próximos passos

1. Revisar AD-008 (decisões do Execution Service não confirmadas pelo usuário).
2. `infra-ci-observabilidade` (pipelines, IaC, telemetria distribuída) e `pacote-entrega` (repositórios, vídeo e PDF final; o vídeo exige ação humana).
3. Decisões pendentes do usuário: criar a `main` dos remotos novos e abrir os PRs; se quer atualizar o texto de `AGENTS.md` ("minúsculas") para refletir AD-002; se prefere um job de reconciliação de reembolsos `Pendente` no Billing.

### Auditoria da PR #45 (10/11/2026)

- A PR `docs/fase4-especificacao-inicial` → `develop` permanece aberta no commit `5e20324`; a branch de trabalho `feature/os-service-contracts` incorporou esse commit no merge `7a2eeeb` e avançou até `036fefd`. A descrição e os três checks vermelhos da PR refletem a versão anterior; o CI parou no checkout do Billing quando o repositório ainda era privado. A URL antiga já redireciona, mas não houve nova execução do CI.
- Os repositórios OS, Cobrança, Execução e Cadastros estão públicos e renomeados onde aplicável; os três serviços novos já têm `main`. Não há PRs nos três serviços nem na branch de Infra.K8s. A `main` de OS está um commit atrás da feature; as de Cobrança e Execução coincidem com as features e nenhuma das três está protegida. Infra.K8s `feature/os-service-contracts` está dois commits à frente de `develop`, sem commits atrás: o rebase pedido na PR já foi feito.
- `saga-mensageria` está PASS, mas `saga-compensacoes`, `insumos`, `cadastros`, `infra-ci-observabilidade` e `pacote-entrega` ainda aguardam aprovação/implementação. Na checklist da PR, a aprovação de `saga-mensageria` já ocorreu; as outras quatro aprovações ainda faltam. A divisão de T1–T5 entre pessoas não foi confirmada no repositório.
- Corrigir o retrato de estado em `especificacao/index.md` (Saga, DynamoDB, contrato, README e branches), a instrução antiga em `especificacao/saga/fase4_spec_saga_pattern.md` que manda reabrir `saga-mensageria` apesar de AD-021, e as referências pessoais a "spec do Enrico" que persistem na descrição da PR e em paráfrases das specs. Harmonizar L-02/D-19 com o contrato implementado `billing.orcamento.cancelar.v1` e o envio SQS por destino; tratar mudanças de contrato com versionamento/compatibilidade.
- `python scripts/specs/verificar_links.py`: 45 arquivos, 189 links, 0 problemas. Esta auditoria não executou build, testes ou deploy.

### Handoff: tipos de eventos em português (10/11/2026)

- D-24/AD-022 concluídos com **PASS** na 1ª rodada do Verifier independente (`author != verifier`, root `e078c04`, OS `45defd3`, Cobrança `8e7bff7`): 13/13 provas verdes e 3/3 mutações mortas (`validate_verification: 0 error(s), 0 warning(s) across [eventos-portugues]`).
- `SagaOS` emite `cobranca.cancelar-orcamento.v1` em compensações novas, normaliza `EtapaSaga.Compensacao` legada sem duplicar etapas e `ComandoSaga.NormalizarTipo()` converte `Tipo` e `Corpo.tipo` de comandos pendentes legados preservando `eventId`, demais campos, backoff e ordem.
- `CancelamentoOrcamentoService` na Cobrança aceita `cobranca.cancelar-orcamento.v1` e o alias legado `billing.orcamento.cancelar.v1`, deduplicando pelo mesmo `eventId` na Inbox (`eventos_processados`) em qualquer ordem de chegada.
- `scripts/specs/verificar_eventos_portugues.py` valida os 4 artefatos de contrato; `scripts/specs/verificar_links.py`: 49 arquivos, 196 links, 0 problemas.
