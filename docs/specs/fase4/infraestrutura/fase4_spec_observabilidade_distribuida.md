# Especificação Fase 4 — Observabilidade e Rastreamento Distribuído

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 4 (15SOAT)  
> **Critério do PDF:** Infraestrutura → *Uso de ferramentas de monitoramento e observabilidade já implementadas na Fase 3* e Vídeo → *Monitoramento e rastreamento dos fluxos distribuídos*  
> **Requisitos cobertos (índice):** R30, R41  
> **Repositórios afetados:** os 4 serviços, `AutoReparos.Infra.K8s` (chart `observability`), `AutoReparos` (pai — `docs/observability/`, `docker-compose.yml`)  
> **Base:** [Diagnóstico do estado atual](../diagnostico/fase4_diagnostico_estado_atual.md) · **Índice:** [`../index.md`](../index.md)  
> **Status:** Rascunho inicial

---

## 1. Critério

> Uso de ferramentas de monitoramento e observabilidade **já implementadas na Fase 3**.
>
> Vídeo: demonstrar **monitoramento e rastreamento dos fluxos distribuídos**.

---

## 2. Estado atual

| Item | Situação |
|---|---|
| SDK | OpenTelemetry no App: traces (ASP.NET Core, HttpClient, EF Core, Npgsql), métricas (runtime, processo, ASP.NET Core, `AutoReparosMetrics`) e logs, exportando OTLP |
| `service.name` | `OpenTelemetry:ServiceName`, padrão `autoreparos-api`; versão fixa `1.0.0` |
| Collector | OTel Collector → Jaeger (traces), Prometheus (métricas), Loki (logs); exportação opcional para New Relic (`otlp/vendor`) |
| Visualização | Grafana com 3 dashboards ([`docs/observability/dashboards/`](../../../observability/dashboards/)): volume diário, tempo médio por status, erros de integrações |
| Alertas | [`docs/observability/alerts/`](../../../observability/alerts/) (regras Prometheus e políticas) |
| Correlação | Logs JSON com `TraceId`/`SpanId`; `CorrelationIdMiddleware` (`X-Correlation-Id`) |
| Métricas de negócio | OS criadas, transições de status, tempos de diagnóstico, execução e permanência, falhas de notificação |
| Onde roda | Chart `observability` no Infra.K8s e `docker-compose.yml` do pai |
| AuthLambda | Sem instrumentação OpenTelemetry |

A base é boa e foi elogiada no feedback da Fase 3, mas foi feita para **um** serviço e **sem mensageria**.

---

## 3. Lacunas

1. Contexto de trace não é propagado em mensagens; um fluxo entre serviços apareceria como traces soltos.
2. Não há identificação por serviço (todos seriam `autoreparos-api`) nem versão real.
3. Não há métricas nem dashboards da Saga, da mensageria ou de pagamentos.
4. Não há alertas para DLQ, compensações ou falhas do Mercado Pago.

---

## 4. Proposta

### 4.1 Instrumentação por serviço

- OS, Billing e Execution já exportam OTLP (traces, métricas e logs) na branch do Enrico; alinhar a configuração com a do Cadastros (App).
- `service.name` distinto: `autoreparos-os`, `autoreparos-cobranca`, `autoreparos-execucao`, `autoreparos-cadastros`; `service.version` = SHA do commit (injetado pelo pipeline); `deployment.environment`.
- Instrumentação da mensageria (SDK da AWS, D-08): instrumentação AWS + injeção e extração manual do `traceparent` no envelope da mensagem; na Saga, o `traceparent` também vai na Outbox do DynamoDB para o trace atravessar o Streams.
- Instrumentação do cliente do Mercado Pago via `HttpClient`.
- AuthLambda (opcional): instrumentação básica ou logs JSON com o correlation id.

### 4.2 Correlação do fluxo da OS

- A Saga tem etapas humanas (diagnóstico, aprovação, pagamento) que podem levar horas, então o fluxo inteiro não cabe em um único trace.
- Cada etapa automática forma um trace que atravessa os serviços (HTTP → mensagem → consumidor).
- O atributo `ordem_servico.id` em todos os spans, logs e métricas une as etapas. No Jaeger, buscar pela tag; no Loki, filtrar pelo campo; no Grafana, usar como variável do dashboard.
- Opcional: *span links* ligando cada etapa à anterior.

### 4.3 Métricas novas

| Métrica | Tipo | Dimensões |
|---|---|---|
| `sagas_iniciadas_total`, `sagas_concluidas_total` | Counter | — |
| `sagas_compensadas_total` | Counter | `passo` |
| `saga_passo_duracao_segundos` | Histogram | `passo` |
| `mensagens_publicadas_total`, `mensagens_consumidas_total`, `mensagens_falhas_total` | Counter | `tipo`, `servico` |
| Profundidade de fila e de DLQ | Gauge | `fila` (CloudWatch) |
| `pagamentos_total` | Counter | `status` |
| `reservas_insumo_falhas_total` | Counter | — |

### 4.4 Dashboards e alertas

| Dashboard | Conteúdo |
|---|---|
| **Saga da OS** (novo) | Funil por etapa, Sagas em andamento, compensações por passo, duração por passo, últimas OS canceladas |
| **Saúde dos microsserviços** (novo) | Taxa, erros e latência por serviço; réplicas e HPA |
| **Mensageria** (novo) | Vazão por fila, falhas, profundidade das DLQs |
| Fase 3 (manter) | Volume diário, tempo médio por status, erros de integrações — consultas ajustadas para os novos nomes de serviço |

Alertas: DLQ com mensagens; taxa de compensação acima do normal; falhas na integração com o Mercado Pago; taxa de erro por serviço; serviço sem réplicas prontas.

Para o vídeo, o grafo de dependências entre serviços do Jaeger (aba *System Architecture*) mostra a comunicação distribuída de forma direta.

---

## 5. Passos para a mudança

1. Padronizar a configuração OpenTelemetry nos 4 serviços com `service.name` próprio e versão real.
2. Ativar a propagação de contexto na mensageria e testar que um comando da Saga aparece no mesmo trace do evento que o originou.
3. Adicionar `ordem_servico.id` como atributo de span e escopo de log em todos os handlers.
4. Implementar as métricas do § 4.3.
5. Criar os dashboards novos em JSON em `docs/observability/dashboards/` e provisioná-los no chart `observability` e no compose local (como na Fase 3).
6. Criar as regras de alerta novas.
7. Ajustar os dashboards da Fase 3 para os novos serviços.
8. Atualizar o ADR-003 (observabilidade) com o desenho distribuído.
9. Preparar um gerador de tráfego que execute o fluxo completo da OS (o roteiro da Fase 3 mostrou que os painéis ficam vazios sem tráfego contínuo).

---

## 6. Critérios de aceite

- [ ] No Jaeger, um passo automático da Saga aparece como um trace único atravessando pelo menos 2 serviços.
- [ ] Buscar por `ordem_servico.id` retorna todos os traces e logs da OS.
- [ ] Dashboards da Saga, de saúde e de mensageria com dados reais.
- [ ] Alerta de DLQ disparando em um teste controlado.
- [ ] Stack da Fase 3 continua funcionando (requisito do PDF).
