# Plano Detalhado — Subfase 3: Dashboards Mandatórios & Alertas (Track B)

> **Projeto:** AutoReparos — Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT — Fase 3  
> **Escopo:** Especificação, Provisionamento Automático de Dashboards Grafana e Regras de Alerta  
> **Status:** Concluído & Auditado  

---

## 1. Contexto & Dashboards Mandatórios

A banca examinadora estabelece 3 dashboards obrigatórios para monitoramento da plataforma:

1. **Volume Diário de Ordens de Serviço (`dashboard_volume_diario.json`):**
   - Total em 30 dias, ordens hoje, ordens/hora e volume diário por status (`Recebida`, `EmDiagnostico`, `AguardandoAprovacao`, `EmExecucao`, `Finalizada`, `Entregue`).
2. **Tempo Médio por Status (`dashboard_tempo_medio_status.json`):**
   - Gauges e séries temporais de tempo médio de diagnóstico, tempo médio de execução e tempo total de permanência.
3. **Erros e Falhas de Integrações (`dashboard_erros_integracoes.json`):**
   - Falhas de notificação, falhas por motivo/canal, erros HTTP 5xx na API e latência do PostgreSQL.

---

## 2. Provisionamento & Fonte Única

- **Fonte Única:** `docs/observability/dashboards/` consumido diretamente por bind mount no Docker Compose local e pelo ConfigMap no Helm Chart do EKS.
- **Datasources com UID Determinístico:** UIDs explícitos (`uid: "Prometheus"`) garantindo resolução automática dos painéis.
- **Políticas de Alerta:** Regras PromQL e JSON equivalentes para latência p95 e taxa de erros em `docs/observability/alerts/`.
