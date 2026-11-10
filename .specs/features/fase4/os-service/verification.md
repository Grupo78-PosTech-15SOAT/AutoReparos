# fase4/os-service verification

**Verdict**: PASS
**Mutations**: 3 - probed C4 (papel Cliente autorizado), C3 (ordem do historico invertida), C2 (limite 2000 da observacao `<=` -> `<` em `CorpoJson.cs`); `proofs.md` registra as 3 mortas e `mutations.json` ainda casa com o codigo (cada `old` ocorre uma vez)
**Proofs**: `proofs.md` at c981b0d
**Verifier**: independent sub-agent (author != verifier)

Terceira rodada, sobre `d9f9cd8..HEAD` do repositorio OS (HEAD e2bbd47, igual ao ponteiro do submodulo em `proofs.md`) e a arvore atual. Nao foram alterados codigo nem testes; nao foram rodados provas, mutacoes nem servicos. Revisao por leitura de `c78041b`, `b17ed6a` e `e2bbd47`.

Resumo: os achados 1 a 9 e 13 a 15 estao fechados com assercoes que fixam valores (status, codigo, estado, outbox e chaves exatas do payload). 10 e 12 foram declarados fora de escopo ou aceitos pelo usuario. 11 esta PARCIAL por decisao: os papeis de Billing e Execucao sem privilegio nas tabelas estao versionados e documentados (`927ee33`), mas o script nao e exercitado por teste do repositorio, C5 depende de ambiente externo (`OS_PROOF_*`) e so consulta SELECT, e a metade do AC 5 via eventos nao tem teste (o README declara isso). Nao ha achado ALTA ou MEDIA aberto; tres achados novos de gravidade BAIXA ficam em `## Open items`. A troca de pacotes OpenTelemetry (`e2bbd47`) so altera o csproj e `Program.cs` nao mudou; build e format de `proofs.md` correspondem a esse HEAD.

## Findings

| Finding | Evidence | Fix | Closed |
| --- | --- | --- | --- |
| 1. Limite de 2000 sem teste de fronteira. FECHADO | `OrderContracts.cs:117-123` (POST com exatamente 2000 aceito e persistido), `:138-155` (PATCH 2000 devolve 200 e persiste; PATCH e POST com 2001 devolvem 422 `ordem_servico_invalida`, estado e outbox inalterados). Mutacao `<=` -> `<` em `CorpoJson.cs` morta | n/a | `05fdab7`, `68a2a59` |
| 2. `veiculoId = Guid.Empty` sem teste. FECHADO | `OrderContracts.cs:164-165` (clienteId e veiculoId vazios) no teste de 422; `:182` exige 0 ordens, 0 historico e 0 outbox; `Program.cs:56-57` rejeita ambos | n/a | `05fdab7` |
| 3. Entrada malformada devolvia 400/415 em vez de 422. FECHADO (os casos NUL e surrogate sao os achados 13 e 14) | `CorpoJson.cs:9-21` e `Program.cs:52-59,70-74` retornam 422 `ordem_servico_invalida`; `OrderContracts.cs:162-181` (corpos do POST) e `:191-206` (corpos do PATCH, estado e outbox inalterados) com `AssertInvalid` checando status e codigo | n/a | `68a2a59`, `05fdab7` |
| 4. PATCH `{}` apagava a observacao. FECHADO | `CorpoJson.cs` devolve `presente = false` e `Program.cs:77` so atualiza e salva quando presente; `OrderContracts.cs:215-218` PATCH `{}` retorna 200 com estado identico e outbox com 1 mensagem; `:219-222` `observacao:null` limpa e gera o 2o evento | n/a | `68a2a59`, `05fdab7` |
| 5. Autorizacao provada so para 1 papel. FECHADO | `OrderContracts.cs:43,107-115` Theory sobre Administrador, Atendente, Mecanico e ServicoOS em POST, GET e PATCH; `AuthorizationContracts.cs:18-22` cobre sem papel (401), Cliente e papel inexistente (403); mutacao Cliente autorizado morta | n/a | `05fdab7` |
| 6. Assercoes anti-vazamento fracas em C4. FECHADO | `AuthorizationContracts.cs:39-68` cria OS real com segredo, exige status esperado em GET, PATCH e POST, nenhum corpo contem id, clienteId, veiculoId, segredo, status nem textos enviados, e a OS e o outbox seguem intactos | n/a | `05fdab7` |
| 7. Swept dizia concorrencia coberta sem teste nem token. FECHADO | `OrdemServicoDbContext.cs:20` `Property<uint>("Version").IsRowVersion()` com migracao e snapshot coerentes; `Program.cs:80-87` devolve 409 `ordem_servico_conflito`; `OrderContracts.cs:248-275` usa `Barrier(2)` para forcar duas leituras antes de escrever, exige um 200 e um 409, estado igual ao vencedor e exatamente 2 eventos no outbox | n/a | `264d237`, `05fdab7` |
| 8. Evento de outbox do PATCH nao provado. FECHADO | `OrderContracts.cs:225-251` apos PATCH exige 2 mensagens, tipo `ordem-servico.atualizada.v1`, id, status, observacao nova, clienteId e veiculoId nao vazios e eventId diferente do da criacao; PATCH 422 e PATCH `{}` nao adicionam mensagem | n/a | `05fdab7` |
| 9. C2 usava `Contains("original")` sobre JSON bruto. FECHADO | `OrderContracts.cs:63-69,130-135` compara o record de estado inteiro (observacao, historico, ordens, outbox) antes e depois e assere observacao original e outbox com 1 mensagem | n/a | `05fdab7` |
| 10. C3 prova ordem apenas com linhas inseridas por SQL; nenhum caminho grava historico alem da criacao. DECLARADO FORA DE ESCOPO | `plan.md:79` (`## Out of scope` com data e fala do usuario); `Program.cs:68` ordena por `RegistradoEm` e o teste de ordem cronologica fixa 3 entradas, mutacao de inversao morta | n/a | Declarado fora de escopo pelo usuario em `plan.md` linha 79: `3fcba4c` |
| 11. C5 nao prova o AC 5 a partir do repositorio. PARCIAL por decisao | Feito: `provisionar-papeis-servicos.sql` cria `autoreparos_billing` e `autoreparos_execucao` sem privilegio e o README documenta o uso. Aberto: nenhum teste do repositorio exercita o script; C5 depende de ambiente externo e so consulta SELECT em 3 tabelas (`ServiceDatabaseIsolationTests.cs:10-38`); nao cobre INSERT/UPDATE nem o papel `ServicoOS`; a via de eventos do AC 5 nao tem teste | n/a | Parcial por decisao: papeis versionados em `927ee33`; lado via eventos do AC 5 sem teste |
| 12. `proofs.md` com filtros truncados e contagens desconhecidas. ACEITO PELO USUARIO | linhas C3, C4 e C5 truncadas com count unknown; os comandos de `checks.md` estao integros e os prefixos truncados selecionam os mesmos testes | n/a | Aceito pelo usuario como limitacao de exibicao do `check_proofs.py`: `3fcba4c` |
| 13. Observacao com NUL passava pela validacao e causava 500 em vez de 422. Rodada 3: FECHADO | `CorpoJson.cs:56-57` rejeita `valor.Contains((char)0)` junto com o limite de 2000; `OrderContracts.cs:179` (POST) e `:205` (PATCH) enviam `a` NUL `b` nos loops de `AssertInvalid` (status 422 e codigo) e em seguida exigem 0 ordens, 0 historico e 0 outbox no POST, ou estado e outbox identicos no PATCH | n/a | `c78041b` |
| 14. Surrogate isolado em observacao, clienteId ou veiculoId causava 500 em vez de 422. Rodada 3: FECHADO | `CorpoJson.cs:37-49` (`TentarLerTexto` captura `InvalidOperationException` de `GetString`) usado por `TentarLerGuid` e `TentarLerObservacao`; `OrderContracts.cs:180-181` (observacao e clienteId no POST) e `:206` (PATCH) com a mesma checagem de status, codigo e estado inalterado | n/a | `c78041b` |
| 15. Contrato do evento de outbox misturava caixa de chaves. Rodada 3: FECHADO | `OrdemServicoDbContext.cs:44` serializa `eventId, tipo, id, clienteId, veiculoId, status, observacao` em camelCase; `OrderContracts.cs:80-81` exige o conjunto exato de 7 chaves e nenhuma iniciando em maiuscula; `:106,247-251,276` leem as chaves novas com valores. `PublicadorSqs.cs` encaminha `Conteudo` opaco (`MessageBody = mensagem.Conteudo`) sem ler chaves; nenhum outro consumidor do PascalCase no repositorio; o README ganhou a secao Contrato do evento com exemplo e `observacao` anulavel, coerente com o codigo | n/a | `b17ed6a` |

## Own examples

- Fronteira da observacao: 2000 aceito e 2001 rejeitado estao fixados em POST e PATCH; 1001 emojis (2002 unidades UTF-16) sao rejeitados, regra conservadora diante de `HasMaxLength(2000)`.
- Valores novos que nem plano nem checks citam: GUID com espacos, chaves ou sem hifens (aceito por `Guid.TryParse`, achado 18) e outbox pendente anterior a `b17ed6a` (achado 16). Propriedades duplicadas usam a primeira ocorrencia e a caixa das propriedades e insensivel, coerente com o binding padrao.
- Concorrencia: dois PATCH simultaneos produzem 200 e 409 sem evento do perdedor; PATCH com o mesmo texto devolve 200 sem evento.

## Open items

- Achado 11 (parcial por decisao): provar o script `provisionar-papeis-servicos.sql` e INSERT/UPDATE em ambiente provisionado, e cobrir a via de eventos do AC 5 numa feature propria (Billing e Execucao consumindo o evento).
- Achado 16 (BAIXA): antes de qualquer deploy com outbox preenchido, drenar ou converter mensagens pendentes em PascalCase para o contrato camelCase do README.
- Achado 17 (BAIXA): registrar no plano ou README que PATCH valida o corpo antes de procurar a OS (422 antes de 404) e que 409 `ordem_servico_conflito` existe.
- Achado 18 (BAIXA): decidir se o contrato exige GUID canonico e documentar que o limite de 2000 conta unidades UTF-16.
- Achados 10 e 12: declarados fora de escopo ou aceitos pelo usuario, sem acao.
