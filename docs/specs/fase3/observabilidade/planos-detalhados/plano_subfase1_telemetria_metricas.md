# Plano Detalhado — Subfase 1: Telemetria de Negócio & Métricas (Track B)

> **Projeto:** AutoReparos — Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT — Fase 3  
> **Escopo:** Instrumentação de Métricas de Negócio com System.Diagnostics.Metrics no Backend .NET 10  
> **Status:** Concluído & Auditado  

---

## 1. Contexto & Requisitos de Negócio

Para atender aos requisitos de observabilidade da Fase 3 e alimentar os dashboards executivos sem sobrecarregar o banco de dados com consultas contínuas, a plataforma implementa métricas de negócio nativas via `System.Diagnostics.Metrics`.

### Instrumentos Mandatórios:
1. `notificacoes.falhas`: Contador de falhas no envio de notificações externas (ex: SendGrid/Email).
2. `ordens_servico.criadas`: Contador do volume de ordens de serviço criadas.
3. `ordens_servico.transicoes_status`: Contador das mudanças de estado no ciclo de vida da OS.
4. Histogramas temporais (`tempo_diagnostico`, `tempo_execucao`, `tempo_permanencia`).

---

## 2. Implementação Técnica

- **Classe Central:** `AutoReparos.Application/Shared/Metrics/AutoReparosMetrics.cs` sob o meter `AutoReparos.BusinessMetrics`.
- **Emissão Automatizada:** `OrdemServicoMetricsInterceptor.cs` no EF Core capturando eventos do `ChangeTracker` após commits bem-sucedidos.
- **Tratamento no Serviço de Notificação:** `NotificacaoService.cs` incrementando falhas com tags contextuais (`canal`, `motivo`, `status_code`).

---

## 3. Matriz de Testes & Validação Empírica

- **Testes Unitários:** `OrdemServicoMetricsInterceptorTests.cs` cobrindo 6 cenários com `MeterListener`.
- **Cobertura Total:** 354 testes na solução passando com 100% de sucesso.
