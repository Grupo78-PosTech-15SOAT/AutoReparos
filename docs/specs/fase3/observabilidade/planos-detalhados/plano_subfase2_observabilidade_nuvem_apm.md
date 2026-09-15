# Plano Detalhado — Subfase 2: Observabilidade Nuvem & APM Vendor (Track B)

> **Projeto:** AutoReparos — Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT — Fase 3  
> **Escopo:** OpenTelemetry Collector, Exportação OTLP para New Relic/Datadog e Logs Estruturados JSON  
> **Status:** Concluído & Auditado  

---

## 1. Contexto & Arquitetura Vendor-Agnostic

O requisito da Fase 3 exige a integração com uma ferramenta de APM de mercado (New Relic ou Datadog). Em vez de acoplar SDKs proprietários ao código C#, a aplicação emite OTLP puro para o OpenTelemetry Collector, que se encarrega do roteamento para os destinos locais e na nuvem.

---

## 2. Implementação Técnica

- **OpenTelemetry Extensions:** `AutoReparos.API/OpenTelemetryExtensions.cs` unificando traces, métricas e logs com o recurso `service.name = autoreparos-api`.
- **OTel Collector Overlay:** Configuração multi-config no Collector (`otel-collector-config.yaml`) adicionando o exportador `otlp/newrelic` com `api-key: ${NEW_RELIC_LICENSE_KEY}` sem substituir os exportadores locais (Loki, Prometheus, Jaeger).
- **Logs Estruturados JSON:** Formatter `JsonConsole` ativado no `Program.cs` com `ActivityTrackingOptions` (TraceId, SpanId e ParentId correlacionados).

---

## 3. Validação Empírica

- **Entrega ao Vendor Validada:** 480 logs, 588 pontos de métrica e 541 spans entregues ao New Relic com zero falhas na autotelemetria do Collector.
