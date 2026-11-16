# Execucao verification

**Verdict**: PASS
**Mutations**: 3 - probed C1 (condicao de deduplicacao por OS, `Contratos.cs:65-67`), C4 (guarda de estado da reexecucao, `OrdemExecucao.cs:57`), C3 (conclusao publicada aos dois destinos, `Contratos.cs:105`); `mutations.json` escrito, nao executado por este Verifier
**Proofs**: `proofs.md` at da5a92f (8 pass, 0 fail; repositorio Execution em HEAD 82d3b8e)
**Verifier**: independent sub-agent (author != verifier)

Lido o codigo de producao inteiro (Domain, Application, Infrastructure, Api) e todos os testes que os checks nomeiam. Exemplos proprios verificados por leitura: borda de 2000 caracteres (aceita 2000, rejeita 2001, em dominio e API), descricao vazia ou so espacos, GUID nao canonico e `eventId` vazio (rejeitados, DLQ com alerta), NUL e surrogate isolado (422 no corpo, DLQ no evento), `sub` ausente (403), dois eventos simultaneos com o mesmo `eventId` e com `eventId` diferente para a mesma OS (uma entrada, indice unico mais `xmin`), reexecucao duas vezes seguidas (409 na segunda), conclusao duas vezes (409, 2 linhas de outbox), etapa em execucao concluida ou falha (409). Nenhuma quebrou. Outbox na mesma transacao da mudanca de estado, conclusao em duas linhas com o mesmo `eventId`, falha de publicacao e DLQ de entrada tem teste que os sustenta. Nenhum achado ALTA ou MEDIA aberto; os BAIXA e riscos conscientes estao em `## Open items`.

## Findings

Nenhum achado ALTA ou MEDIA. Os 5 achados BAIXA abaixo ficam em `## Open items` e nao bloqueiam o PASS.

## Open items

Achados BAIXA do Verifier:

- **BAIXA - Contrato de integracao nao fecha com os outros servicos: o OS Service so publica `ordem-servico.criada.v1` e `atualizada.v1`, nunca `ordem-servico.liberada.v1`, e nem OS nem Billing consomem `execucao.concluida.v1` ou `execucao.falhou.v1`. Se o OS publicar na fila de entrada, cada mensagem vai para a DLQ com alerta `execucao.evento_invalido`** - Evidencia: `AutoReparos.OS.Infrastructure/OrdemServicoDbContext.cs:39` (tipos emitidos); `Contratos.cs:28` e `Contratos.cs:8` (tipo exigido); busca por `execucao.concluida` fora do repositorio Execution sem resultado Sugestao: produtor do evento de liberacao no OS e consumidores de conclusao no OS e no Billing (feature Saga ou integracao); ate la o README deve dizer que o contrato e unilateral
- **BAIXA - C5 nao distingue a checagem de aplicacao da restricao do banco: sem o `if EventoProcessadoAsync` a reentrega do mesmo evento cairia em violacao de chave unica, seria tratada como Duplicada e todos os testes de C5 continuariam verdes** - Evidencia: `Contratos.cs:63` e `DeduplicacaoTests.cs:13-64` (assercoes em contagens de linhas e remocoes, iguais nos dois caminhos); `Contratos.cs:68-74` Sugestao: defesa em profundidade aceitavel; se quiser provar a checagem, assertar que nenhuma violacao ocorreu, por exemplo contando chamadas ao `SalvarAsync` por entrega repetida
- **BAIXA - Backoff do outbox e dos laços de segundo plano sem teste de crescimento nem de teto: a unica asseracao e `ProximaTentativaEm > agora + 1s`** - Evidencia: `ResilienciaTests.cs:176-177`; formula em `Integracoes.cs:125` e `ServicosSegundoPlano.cs:22-24`, `:47-49` Sugestao: testar `Tentativas` 1, 2 e 7 com relogio fixo ou extrair a formula para `RetryDelayPolicy` e cobri-la como a primeira camada
- **BAIXA - `sub` em branco, maior que 200 ou com NUL vira 422 com codigo `etapa_invalida` ou `reexecucao_invalida` (o corpo e valido), em vez de 403** - Evidencia: `Program.cs:81` aceita qualquer `sub` nao vazio; `Tipos.cs:23` e `Executar` em `Program.cs:104` convertem o `ArgumentException` Sugestao: tratar identidade invalida como 403 antes de ler o corpo; testar `sub` de 201 caracteres
- **BAIXA - Apos reexecucao, `MotivoFalha` e `FalhouEm` continuam preenchidos com a falha antiga enquanto o estado volta a `EmExecucao`** - Evidencia: `OrdemExecucao.cs:55-63` (nao limpa); `Program.cs:92-95` projeta ambos na fila Sugestao: limpar na reexecucao (o historico fica em `Reexecucoes`) ou documentar que sao da ultima falha

Outros itens:

- **Alerta ao esgotar fora do consumidor** (risco consciente, BAIXA): o AC 6 pede limites, backoff com jitter e DLQ ou alerta ao esgotar. O consumidor de entrada (banco) e o publicador SQS cumprem e tem teste. Mas a API devolve 503 sem retry nem alerta (aceito como no Billing), o despachante do outbox quando o banco cai e o `ReceiveMessage` do SQS so registram log e repetem com backoff sem teto e sem alerta (`ServicosSegundoPlano.cs:19-25`, `:44-49`). Ligar um alerta apos N falhas seguidas.
- **Mensagem de entrada com falha nao transitoria** (risco consciente, BAIXA): a excecao propaga, a mensagem nao e removida e o lote restante e abandonado ate o timeout de visibilidade (`Integracoes.cs:141`, `:154-166`); sem politica de redrive na fila de entrada ela repete para sempre e sem alerta. Documentar a exigencia de `maxReceiveCount` na fila ou alertar.
- **Fila sem paginacao** (BAIXA): `GET /api/v1/fila-execucao` devolve todas as ordens abertas com todas as etapas (`ExecucaoDbContext.cs:60-66`, `Program.cs:117-118`). Limitar por pagina antes de o volume crescer.
- **Filtro `mecanico`** (BAIXA, desenho): usa `Etapas.Any(Responsavel == mecanico)` (`ExecucaoDbContext.cs:64`), entao uma OS na `Fila` sem etapa nunca aparece para um mecanico filtrado. Confirmar que e a intencao.
- **Timeouts e retries do SDK**: nenhuma configuracao explicita de timeout do `AmazonSQSClient` nem dos retries internos do SDK (`Program.cs:40`), que se somam as 5 tentativas da politica; durante as tentativas o publicador segura a transacao e a trava da linha do outbox (`Integracoes.cs:110-117`). Fixar `Timeout` e `MaxErrorRetry`.
- **Rotas fora do `## Surface`**: `POST /execucoes/{id}/conclusao` e `/falha` existem (`Program.cs:137`, `:143`), e as rotas do plano tambem respondem 409 `conflito_concorrencia` e 503 `dependencia_indisponivel`. Atualizar o plano ou a cobertura.
- **Entrega pelo menos uma vez na saida**: se o `SaveChanges` do despachante falhar depois do envio, a linha e reenviada (`Integracoes.cs:117-129`); os consumidores precisam deduplicar pelo `eventId`, que e o mesmo nas duas linhas de uma conclusao.
- **`eventos_processados` cresce sem purga** e `mensagens_saida` enviadas nao sao removidas (BAIXA).
- **Docker** e necessario para todos os proofs (Testcontainers `postgres:16-alpine`).
