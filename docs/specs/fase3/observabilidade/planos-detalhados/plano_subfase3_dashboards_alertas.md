# Plano Detalhado de Implementação — Subfase 3: Dashboards Mandatórios & Políticas de Alerta (Track B)

> **Projeto:** AutoReparos — Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT — Fase 3  
> **Escopo:** Especificação JSON dos Dashboards Grafana, Provisionamento Automático (Local & Helm) e Regras de Alerta  
> **Diretório Alvo:** `docs/observability/dashboards/` e `docs/observability/alerts/`  
> **Documento Mestre:** [`../plano_execucao_track_b_observabilidade.md`](../plano_execucao_track_b_observabilidade.md)  
> **Status:** Concluído & Auditado  

---

## 1. Contexto de Negócio & Justificativa Técnica

### 1.1. Os Três Pilares Visuais Exigidos pela Banca
O edital do Tech Challenge FIAP SOAT (Fase 3) define três dimensões analíticas que a oficina mecânica deve obrigatoriamente monitorar através de dashboards executivos:
1. **Volume Diário de Ordens de Serviço:** Visibilidade do fluxo de entrada de veículos e distribuição do trabalho pelas diferentes fases do ciclo de atendimento (`Recebida`, `EmDiagnostico`, `AguardandoAprovacao`, `EmExecucao`, `Finalizada`, `Entregue`).
2. **Tempo Médio por Status:** Indicadores de eficiência operacional que permitem aos gerentes identificar gargalos entre a recepção, o tempo que o mecânico leva no diagnóstico, a espera pela decisão do cliente e o tempo real de reparo.
3. **Erros e Falhas nas Integrações:** Monitoramento de confiabilidade técnica e resiliência, detectando indisponibilidade de serviços externos (SendGrid), falhas internas de servidor (erros HTTP 5xx) e lentidão nas consultas do banco de dados relacional.

### 1.2. O Desafio do Provisionamento sem Duplicação
Em muitas soluções acadêmicas, os dashboards JSON são exportados da interface do Grafana e versionados em pastas aleatórias, exigindo importação manual a cada inicialização de ambiente ou divergindo entre o ambiente local (Docker Compose) e o cluster de produção (Kubernetes EKS).
- **Abordagem de Fonte Única:** Os arquivos declarativos residem exclusivamente no repositório em `docs/observability/dashboards/`.
- **Injeção Local:** O `docker-compose.yml` monta essa pasta como volume de leitura no diretório de autovisionamento do Grafana (`/etc/grafana/provisioning/dashboards/json/mandatorios:ro`).
- **Injeção no Kubernetes:** O Helm chart de observabilidade (`autoreparos-grafana-dashboards-configmap.yaml`) empacota exatamente as mesmas definições JSON no ConfigMap consumido pelo pod do Grafana no cluster EKS.

---

## 2. Matriz de Decisões Técnicas Alinhadas

| Dimensão | Decisão Adotada | Racional Técnico | Alternativa Descartada |
|:---|:---|:---|:---|
| **Fonte de Dados dos Painéis** | Métrica OTel com Prometheus (`autoreparos_*`) | Consome diretamente as séries temporais leves agregadas pelo runtime .NET e driver Npgsql. | Consultas SQL diretas nas tabelas de OS, que causariam concorrência com o banco operacional. |
| **Resolução de Datasource** | UID Determinístico (`uid: "Prometheus"`) | Garante que os painéis consigam vincular-se à fonte de dados automaticamente em qualquer ambiente, sem depender de UIDs aleatórios gerados pelo Grafana. | Deixar o Grafana gerar UIDs automáticos (ex.: `PBFA97CFB590B2093`), o que deixava os painéis em branco. |
| **Janelas de Agregação PromQL** | `[5m]` para `rate()` e `[1d]` para `increase()` | Como o SDK do OpenTelemetry exporta em ciclos de 60 segundos com carimbos fixos, janelas curtas de `[1m]` contêm apenas uma amostra e falham silenciosamente (retornam vazio). | Janelas de `[1m]` que provocavam gráficos intermitentes e taxas de erro incorretas. |
| **Políticas de Alerta Duais** | PromQL (Prometheus) e NRQL (New Relic) equivalentes | Assegura que o sistema possa alarmar incidentes operacionais tanto na infraestrutura local/on-premise quanto na nuvem corporativa. | Alertas exclusivos de uma ferramenta proprietária. |

---

## 3. Especificação Técnica dos Dashboards Mandatórios

### 3.1. Dashboard 1: Volume Diário de Ordens de Serviço
Arquivo: `docs/observability/dashboards/dashboard_volume_diario.json` (UID: `autoreparos-volume-diario`)

| Painel | Tipo Visual | Expressão PromQL | Finalidade |
|:---|:---:|:---|:---|
| **Total nos Últimos 30 Dias** | Stat | `sum(increase(autoreparos_ordens_servico_criadas_total[30d]))` | Volume consolidado no período de faturamento. |
| **Ordens Criadas Hoje** | Stat | `sum(increase(autoreparos_ordens_servico_criadas_total[1d]))` | Acompanhamento do ritmo de entrada do dia. |
| **Ordens Criadas por Hora** | Stat | `sum(rate(autoreparos_ordens_servico_criadas_total[1h])) * 3600` | Taxa de chegada instantânea por hora. |
| **Volume Diário por Status** | Bar Chart | `sum by (status) (increase(autoreparos_ordens_servico_transicoes_status_total[1d]))` | Distribuição diária empilhada por status Kanban. |
| **Evolução do Volume de Criação** | Time Series | `sum(increase(autoreparos_ordens_servico_criadas_total[1d]))` | Tendência histórica de novos atendimentos. |

### 3.2. Dashboard 2: Tempo Médio por Status da Ordem de Serviço
Arquivo: `docs/observability/dashboards/dashboard_tempo_medio_status.json` (UID: `autoreparos-tempo-medio-status`)

*Valores exibidos em horas (`unit: "h"`)*:
- **Tempo Médio de Diagnóstico (Gauge):**
  $$\text{expr} = \frac{\sum(\text{increase}(\text{tempo\_diagnostico\_seconds\_sum}[30d]))}{\text{clamp\_min}(\sum(\text{increase}(\text{tempo\_diagnostico\_seconds\_count}[30d])), 1) \times 3600}$$
- **Tempo Médio de Execução (Gauge):**
  $$\text{expr} = \frac{\sum(\text{increase}(\text{tempo\_execucao\_seconds\_sum}[30d]))}{\text{clamp\_min}(\sum(\text{increase}(\text{tempo\_execucao\_seconds\_count}[30d])), 1) \times 3600}$$
- **Tempo Médio de Permanência Total (Gauge):**
  $$\text{expr} = \frac{\sum(\text{increase}(\text{tempo\_permanencia\_seconds\_sum}[30d]))}{\text{clamp\_min}(\sum(\text{increase}(\text{tempo\_permanencia\_seconds\_count}[30d])), 1) \times 3600}$$
- **Amostras Computadas (Stat):** Contadores das ordens de serviço finalizadas que alimentaram as médias móveis.

### 3.3. Dashboard 3: Erros e Falhas nas Integrações
Arquivo: `docs/observability/dashboards/dashboard_erros_integracoes.json` (UID: `autoreparos-erros-integracoes`)

| Painel | Tipo Visual | Expressão PromQL | Finalidade |
|:---|:---:|:---|:---|
| **Taxa de Falhas de Notificação** | Time Series | `sum(rate(autoreparos_notificacoes_falhas_total[5m]))` | Detecção imediata de problemas no envio de e-mails SendGrid. |
| **Falhas por Canal e Motivo** | Pie Chart | `sum by (canal, tipo, motivo, status_code) (increase(autoreparos_notificacoes_falhas_total[24h]))` | Diagnóstico de causa raiz (ex.: `HttpRequestException`, HTTP `401`). |
| **Respostas HTTP 5xx na API** | Time Series | `sum by (http_route, http_response_status_code) (rate(autoreparos_http_server_request_duration_seconds_count{http_response_status_code=~"5.."}[5m])) or vector(0)` | Erros não tratados na camada web. |
| **Taxa de Erro 5xx (%)** | Gauge | `100 * (sum(rate(5xx[5m])) or vector(0)) / clamp_min(sum(rate(total[5m])), 0.001)` | Porcentagem relativa de requisições degradadas. |
| **Latência do Banco de Dados** | Time Series | `histogram_quantile(0.95, sum by (le) (rate(autoreparos_db_client_operation_duration_seconds_bucket[5m]))) * 1000` | Percentil 95 da duração das queries PostgreSQL via Npgsql. |

---

## 4. Políticas de Alerta Automatizadas

Localização: `docs/observability/alerts/alert_policies.json` e `prometheus_alert_rules.yml`

As regras estão estruturadas com thresholds de proteção contra falsos-positivos:

```yaml
groups:
  - name: autoreparos-confiabilidade
    interval: 30s
    rules:
      - alert: FalhasIntegracaoNotificacao
        expr: sum(rate(autoreparos_notificacoes_falhas_total[1m])) > 0
        for: 3m
        labels:
          severity: critical
        annotations:
          summary: "Falhas no envio de notificações ao cliente"

      - alert: LatenciaExcessivaAPI
        expr: histogram_quantile(0.95, sum by (le) (rate(autoreparos_http_server_request_duration_seconds_bucket[5m]))) * 1000 > 2000
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Latência p95 acima de 2000 ms"

      - alert: TaxaErroServidor
        expr: 100 * (sum(rate(autoreparos_http_server_request_duration_seconds_count{http_response_status_code=~"5.."}[5m])) or vector(0)) / clamp_min(sum(rate(autoreparos_http_server_request_duration_seconds_count[5m])), 0.001) > 5
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "Taxa de erro 5xx acima de 5%"
```

---

## 5. Análise de Riscos, Mitigações e Rollback

| Risco Técnico Identificado | Severidade | Probabilidade | Mitigação Arquitetural | Procedimento de Rollback |
|:---|:---:|:---:|:---|:---|
| Escape inválido no Helm quebrando JSON dos dashboards | Alta | Média | Utilização do padrão `{{ "{{" }}status}}` no template ConfigMap do Kubernetes. | Restaurar o template Helm anterior e recompilar com `helm template`. |
| Gráficos em branco por ausência de tráfego contínuo | Média | Alta | Documentação expressa no checklist pré-gravação exigindo execução do script de tráfego 10 min antes. | Executar o gerador de tráfego fornecido no roteiro do vídeo. |
| Divergência de nomes de métricas com e sem prefixo | Alta | Baixa | Nomes no JSON padronizados com o prefixo `autoreparos_` gerado pelo OTel Collector. | Revalidar expressões contra a rota `/metrics` do Prometheus. |

---

## 6. Critérios de Aceite & Definition of Done (DoD)

- [x] Os 3 arquivos JSON mandatórios criados e validados em `docs/observability/dashboards/`.
- [x] Dashboards provisionados deterministicamente no Grafana local (`localhost:3000`) sem import manual.
- [x] Helm Chart do Kubernetes (`AutoReparos.Infra.K8s`) atualizado com os novos dashboards no ConfigMap.
- [x] Regras de alerta documentadas em formato PromQL (`prometheus_alert_rules.yml`) e JSON multi-cloud (`alert_policies.json`).
- [x] Teste de estresse de erro real executado derrubando o banco e confirmando a reação dos painéis 5xx.
