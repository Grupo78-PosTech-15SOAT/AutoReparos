# Infraestrutura, CI/CD e observabilidade checks

Mutations: 3
Build: `dotnet build submodules/AutoReparos.OS/AutoReparos.OS.slnx`; `dotnet build submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.slnx`; `dotnet build submodules/AutoReparos.Execucao/AutoReparos.Execucao.slnx`; `dotnet build submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.slnx`; `python3 scripts/specs/assert_deploy.py --check iac-formatado`
Plan: `.specs/features/fase4/infra-ci-observabilidade/plan.md`

26 checks in 4 slices · 5 one-way doors · 0 open, of which 0 block

Checks de 10/11 a 12/11 (C1 a C14 antigos, com staging e deploy) foram abandonados com a revisão D-68; a numeração abaixo recomeça.

## Checks

### S1 - Trace único entre os serviços

**C1** - No OS, um comando da Saga criado com uma `Activity` corrente grava no `Corpo` o `traceparent` igual a `Activity.Current.Id` (W3C) e o `tracestate` da `Activity`; sem `Activity` corrente, o `Corpo` não tem o campo e o comando é gravado (AC 5.1)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TraceMensageriaTests`

**C2** - Na Cobrança, a mensagem de Outbox gravada ao aprovar o orçamento e o `pagamento.resultado.v1` enviado pelo `SqsResultPublisher` levam o `traceparent` da `Activity` corrente (AC 5.1)
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~TraceMensageriaTests`

**C3** - Na Execução, a mensagem de Outbox gravada ao concluir a execução leva o `traceparent` da `Activity` corrente (AC 5.1)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~TraceMensageriaTests`

**C4** - O publicador de cada produtor (`SqsEnvioComandos` no OS, `SqsEnvio` na Cobrança, `PublicadorSqsEventos` na Execução) registra um span de kind `Producer` chamado `publicar <tipo>` no `ActivitySource` `AutoReparos.<Servico>.Mensageria`, com o mesmo `TraceId` do envelope (AC 5.2)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TraceMensageriaTests`
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~TraceMensageriaTests`
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~TraceMensageriaTests`

**C5** - O consumidor de cada serviço (`SagaConsumidor`, `ComandosConsumer`, `ConsumidorOsLiberada`, `ConsumirFilaNotificacoesUseCase`) processa uma mensagem com `traceparent` válido dentro de um span de kind `Consumer` chamado `processar <tipo>`, filho do contexto do envelope e com o atributo `ordem_servico.id` igual ao `ordemServicoId` do envelope (AC 5.3)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TraceMensageriaTests`
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~TraceMensageriaTests`
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~TraceMensageriaTests`
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Application.Tests --filter FullyQualifiedName~TraceNotificacoesTests`

**C6** - No OS, consumir `execucao.concluida.v1` com trace T grava o comando seguinte da Saga com `traceparent` de trace T (o trace continua pela Saga) (AC 5.3)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TraceMensageriaTests`

**C7** - Uma mensagem sem `traceparent`, com `traceparent` = `"abc"` ou com um campo desconhecido no envelope é processada como hoje, num span raiz novo, e não vai para a DLQ, nos quatro consumidores (AC 5.4)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TraceMensageriaTests`
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~TraceMensageriaTests`
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~TraceMensageriaTests`
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.Application.Tests --filter FullyQualifiedName~TraceNotificacoesTests`

**C8** - O pipeline de traces de cada serviço registra o `ActivitySource` `AutoReparos.<Servico>.Mensageria` (OS, Cobrança e Execução no `Program.cs`; Cadastros em `OpenTelemetryExtensions`) (AC 5.2, AC 5.3)
Proof: `python3 scripts/specs/assert_observabilidade.py --check fontes`

**C9** - Em produção, depois da aprovação de um orçamento e da conclusão da execução de uma OS, o Jaeger mostra pelo menos um trace com spans de pelo menos dois serviços (OS e Execução, ou OS e Cobrança) sob o mesmo `TraceId` (AC 5.5)
Proof: walk - rodar o checkout de ponta a ponta de `deploy-servicos/smoke.md`, abrir o Jaeger (`kubectl port-forward svc/autoreparos-jaeger`), buscar pela tag `ordem_servico.id` e registrar o `TraceId` e os serviços em `smoke.md`

### S2 - Dashboard da Saga

**C10** - O `Meter` `AutoReparos.OS.Saga` soma 1 em `sagas_iniciadas_total` ao iniciar uma Saga e 1 em `sagas_concluidas_total` ao concluí-la (AC 5.6)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TelemetriaSagaTests`

**C11** - Uma liberação recusada que cancela o orçamento e a reserva soma 1 em `sagas_compensadas_total` para cada comando de compensação despachado, com a dimensão `passo` igual ao tipo do comando (AC 5.6)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TelemetriaSagaTests`

**C12** - O gauge `sagas_paradas` publica o número de Sagas não terminais com prazo vencido pela mesma regra do console de Sagas, e o gauge `saga_dlq_mensagens` publica o `ApproximateNumberOfMessages` da `autoreparos-os-saga-dlq` (AC 5.7)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TelemetriaSagaTests`

**C13** - Se a leitura da DLQ falhar, o gauge `saga_dlq_mensagens` não publica amostra naquela coleta e nenhuma exceção sai do callback (AC 5.7)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter FullyQualifiedName~TelemetriaSagaTests`

**C14** - O pipeline de métricas do OS registra o `Meter` `AutoReparos.OS.Saga`, e a IRSA do OS permite `sqs:GetQueueAttributes` na `autoreparos-os-saga-dlq` (AC 5.7)
Proof: `python3 scripts/specs/assert_observabilidade.py --check meter-os`
Proof: `python3 scripts/specs/simular_iam.py --caso irsa`

**C15** - O chart de observabilidade renderizado tem os datasources `Prometheus`, `Jaeger` e `Loki` com as URLs `http://autoreparos-prometheus:9090`, `http://autoreparos-jaeger:16686` e `http://autoreparos-loki:3100`, e o dashboard `Saga` com os painéis `Sagas iniciadas`, `Sagas concluídas`, `Compensações por passo`, `Sagas paradas` e `DLQ da Saga` (AC 5.8)
Proof: `python3 scripts/specs/assert_observabilidade.py --check grafana`

**C16** - Em produção, depois de uma OS completar a Saga, o painel `Sagas concluídas` do dashboard `Saga` mostra valor maior que 0 na última hora e os três datasources respondem no Grafana (AC 5.9)
Proof: walk - abrir o Grafana por port-forward depois do checkout de ponta a ponta, conferir o painel e o "Test" de cada datasource, registrar em `smoke.md`

### S3 - SNS mínimo da Execução

**C17** - A Execução grava uma linha de Outbox por evento e publica cada uma com uma única chamada `sns:Publish` no ARN de `Sns:EventosTopicArn`, com o envelope inalterado e o atributo de mensagem `tipo` igual ao `tipo` do envelope; uma linha pendente com o `Destino` antigo também é publicada no tópico (AC 5.10)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~PublicacaoSnsTests`

**C18** - Se o `sns:Publish` falhar, a linha continua pendente e é tentada de novo com o backoff existente, sem ir para a DLQ antes de esgotar as tentativas (AC 5.10, AC 6)
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~PublicacaoSnsTests`

**C19** - O Terraform da Execução cria o tópico `autoreparos-execucao-eventos`, dá `sns:Publish` nele à IRSA da Execução e o Helm entrega o ARN em `Sns__EventosTopicArn` (AC 5.10)
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.Execucao/infra/terraform sns`
Proof: `python3 scripts/specs/assert_observabilidade.py --check sns-helm`

**C20** - O Terraform do OS assina a `autoreparos-os-saga` no tópico e o do Cadastros assina a `autoreparos-cadastros-notificacoes`, ambos com `raw_message_delivery = true`, `filter_policy` em `tipo` (OS: os tipos de entrada da Saga que a Execução produz; Cadastros: só `execucao.concluida.v1`) e `aws_sqs_queue_policy` que só aceita `sqs:SendMessage` de `sns.amazonaws.com` com `aws:SourceArn` igual ao ARN do tópico (AC 5.11)
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.OS/infra/terraform assinatura-execucao`
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.Cadastros/infra/terraform assinatura-execucao`

**C21** - O filtro do OS contém exatamente os tipos que a Execução publica e que `TiposMensagem` do OS lista como entrada (nem um a mais, nem um a menos), e o do Cadastros contém `execucao.concluida.v1` e está entre os tipos que `EventoNotificacao` aceita (AC 5.11)
Proof: `python3 scripts/specs/assert_observabilidade.py --check filtros-sns`

**C22** - O IAM nega e permite SNS por prefixo do serviço: IRSA da Cobrança publicando no tópico da Execução → negado; role de deploy do OS criando `autoreparos-execucao-x` → negado; IRSA da Execução publicando no próprio tópico → permitido; role de deploy da Execução criando `autoreparos-execucao-eventos` → permitido; role de deploy do OS assinando `autoreparos-execucao-eventos` → permitido (AC 5.12)
Proof: `python3 scripts/specs/simular_iam.py --caso sns`
Proof: `scripts/specs/tf_test.sh submodules/AutoReparos.Infra.K8s/terraform/stacks/base roles-deploy`

**C23** - Em produção, depois do deploy do SNS, uma execução concluída faz a Saga do OS avançar, o Cadastros registra o processamento de `execucao.concluida.v1`, `aws sns list-subscriptions-by-topic` lista as duas assinaturas e as DLQs `autoreparos-os-saga-dlq` e `autoreparos-cadastros-notificacoes-dlq` ficam com 0 mensagens (AC 5.13)
Proof: walk - concluir uma execução no checkout de ponta a ponta, conferir a Saga pela API do OS, o log do Cadastros, as assinaturas e as DLQs, registrar em `smoke.md`

### S4 - CI, proteção da `main` e resiliência

**C24** - A última execução do `ci-cd.yml` na `main` de OS, Cobrança, Execução e Cadastros terminou com `build`, `test` e `quality` em `success`, e o `test` de cada um falha abaixo de 80% de cobertura de linhas (AC 1)
Proof: `python3 scripts/specs/assert_pipelines.py --check execucao-main`
Proof: `python3 scripts/specs/assert_pipelines.py --check cobertura`

**C25** - A `main` de OS, Cobrança, Execução, Cadastros e Web exige PR com 1 aprovação, `dismiss_stale_reviews`, status checks `strict` com `build` e `test`, bloqueia force push e exclusão e tem `enforce_admins` desligado; um PR com teste quebrado fica com o merge bloqueado pelo check `test` (AC 2)
Proof: `python3 scripts/specs/assert_branch_protection.py --check main`
Proof: walk - abrir um PR de demonstração com um teste quebrado num repositório protegido, conferir o merge bloqueado e fechar sem merge

**C26** - Mensagem que falha até o limite é tentada de novo com backoff exponencial e jitter e, ao esgotar, vai para a DLQ da fila, nos quatro serviços (AC 6)
Proof: `dotnet test submodules/AutoReparos.OS/AutoReparos.OS.Tests --filter "FullyQualifiedName~SagaRetryTests|FullyQualifiedName~SagaDlqRotaTests"`
Proof: `dotnet test submodules/AutoReparos.Cobranca/AutoReparos.Cobranca.Tests --filter FullyQualifiedName~BillingMensageriaTests`
Proof: `dotnet test submodules/AutoReparos.Execucao/AutoReparos.Execucao.Tests --filter FullyQualifiedName~ResilienciaTests`
Proof: `dotnet test submodules/AutoReparos.Cadastros/AutoReparos.Cadastros.IntegrationTests --filter FullyQualifiedName~FalhaDeEmailRepeteEVaiParaDlq`

## Coverage

| Set (size) | Member -> proof | Unproven |
| --- | --- | --- |
| produtores de envelope (4) | OS `ComandoSaga` C1 · Cobrança Outbox C2 · Cobrança `SqsResultPublisher` C2 · Execução Outbox C3 | - |
| publicadores (3) | `SqsEnvioComandos` C4 · `SqsEnvio` C4 · `PublicadorSqsEventos` C4, C17 | - |
| consumidores (4) | `SagaConsumidor` C5, C6 · `ComandosConsumer` C5 · `ConsumidorOsLiberada` C5 · `ConsumirFilaNotificacoesUseCase` C5 | - |
| AC 5.4, exemplos (3) | sem `traceparent` C7 · `traceparent` = `"abc"` C7 · campo desconhecido C7 | - |
| startup config: `ActivitySource` (4 assemblies) | OS `Program.cs` C8 · Cobrança `Program.cs` C8 · Execução `Program.cs` C8 · Cadastros `OpenTelemetryExtensions` C8 | - |
| startup config: `Meter` da Saga (1 assembly) | OS `Program.cs` C14 | - |
| métricas da Saga (5) | `sagas_iniciadas_total` C10 · `sagas_concluidas_total` C10 · `sagas_compensadas_total{passo}` C11 · `sagas_paradas` C12 · `saga_dlq_mensagens` C12, C13 | - |
| datasources (3) | Prometheus C15 · Jaeger C15 · Loki C15 | - |
| painéis do dashboard `Saga` (5) | `Sagas iniciadas` C15 · `Sagas concluídas` C15, C16 · `Compensações por passo` C15 · `Sagas paradas` C15 · `DLQ da Saga` C15 | - |
| AC 5.10, exemplos (2) | `execucao.concluida.v1` uma publicação C17 · linha com `Destino` antigo C17 | - |
| AC 5.11, exemplos (3) | `insumos.reservados.v1` só OS C20, C21 · `execucao.concluida.v1` OS e Cadastros C20, C21 · tipo fora do filtro nenhuma fila C20 | - |
| AC 5.12, exemplos (5) | IRSA Cobrança → tópico Execução negado C22 · deploy OS → criar `autoreparos-execucao-x` negado C22 · IRSA Execução → próprio tópico permitido C22 · deploy Execução → criar tópico permitido C22 · deploy OS → assinar tópico permitido C22 | - |
| Landing doors (5) | 1 tópico C19 · 2 assinatura pelo consumidor C20 · 3 IAM base C22 · 4 campos do envelope C1, C2, C3, C7 · 5 proteção da `main` C25 | - |
| repositórios do AC 1 (4) | OS C24 · Cobrança C24 · Execução C24 · Cadastros C24 | - |
| repositórios do AC 2 (5) | OS C25 · Cobrança C25 · Execução C25 · Cadastros C25 · Web C25 | - |
| serviços do AC 6 (4) | OS C26 · Cobrança C26 · Execução C26 · Cadastros C26 | - |
| provas em produção (3) | trace C9 · dashboard C16 · SNS C23 | - |

## Swept

- validation: C7
- failure modes: C13, C18, C26
- idempotency: existing - Inbox por `eventId` no OS (`TabelaEventos`, `OrquestradorSaga`) e `eventos_processados`/`NotificacaoEnviada` nos demais; a linha antiga publicada no tópico (C17) é deduplicada por ele
- authorization: C22
- concurrency: existing - publicadores da Outbox com `FOR UPDATE SKIP LOCKED` (Cobrança e Execução) e `TransactWriteItems` no OS; spans e contadores não mudam a ordem de processamento
- data lifecycle: C7 (mensagens já gravadas sem `traceparent`); n/a para persistência de traces - Jaeger em memória, fora de escopo
- dependency failure: C13, C18
- state transitions: C10, C11
- observability: C1-C16

## Handoff

- **Settled mid-build:** none
- **Dispatched:** builders do S1 (trace) em 16/11 a partir das `main` (OS `f2b946b`, Cobrança `aa06062`, Execução `ef92b33`, Cadastros `4a7cd00`), um por submódulo, branch `feature/mensageria-rastreavel`
- **Boundary:** checks derivados do plano aprovado em 16/11 (`9fc2230`); build não iniciado
- **Abandoned:** C1 a C14 de 10/11 a 12/11 (staging, deploy e IaC por serviço passaram para `deploy-servicos`, D-40, D-41, D-68)
- **Walked:** none
