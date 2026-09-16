# Especificação Técnica Fase 3 - Track B: Observabilidade, Métricas de Negócio, Dashboards & Arquitetura de Aplicação

> **Projeto:** AutoReparos - Sistema Integrado de Oficina Mecânica  
> **Fase:** Tech Challenge FIAP SOAT - Fase 3  
> **Papel / Responsável:** **Engenheiro de Software, Observabilidade & Arquitetura (Track B)**  
> **Repositórios de Atuação Principal:**
> 1. [`AutoReparos.App`](https://github.com/Grupo78-PosTech-15SOAT/AutoReparos.App) (Backend .NET 10, OpenTelemetry, Healthchecks, Logging)
> 2. Repositório Central (`AutoReparos`) - Testes, Dashboards, Telemetria & Documentação Formal  
> **Status:** Pronto para Implementação Paralela e Assíncrona  
> **Documento Complementar:** [`fase3_spec_track_a_cloud_iac.md`](../cloud-iac/fase3_spec_track_a_cloud_iac.md) (Track A: Infraestrutura Cloud, IaC Terraform, API Gateway & Multi-Repo CI/CD)

---

## 1. Visão Geral & Estratégia de Paralelismo

Esta especificação define o escopo integral de **Observabilidade (OpenTelemetry + New Relic/Datadog), Logs Estruturados em JSON, Métricas de Negócio, Dashboards Mandatórios, Alertas Automatizados, Documentação Arquitetural de Software/Dados e Roteiro de Entrega Final**.

### Princípios de Trabalho Paralelo e Assíncrono com o Track A:
1. **Isolamento de Código:** Todo o trabalho de código do Track B ocorre nas camadas de backend (.NET 10), serviços de telemetria, dashboards e suítes de testes (`AutoReparos.Application`, `AutoReparos.Infra`, `AutoReparos.API`, `AutoReparos.App`). Nenhuma alteração em arquivos Terraform ou configurações de rede AWS é realizada aqui.
2. **Ambiente Local 100% Autônomo:** O Track B **não depende** da nuvem AWS ou do cluster EKS estar online. Toda a validação de telemetria, logs, métricas e testes de integração roda localmente via `docker-compose.yml`, contêineres de teste via **Testcontainers** (`Testcontainers.PostgreSql`), e OTel Collector conectado à conta de teste do New Relic ou Datadog.
3. **Contrato de Consumo Estável:**
   - O backend lê a connection string via `ConnectionStrings__DefaultConnection` (em teste, provida pelo Testcontainers; em produção, pelo segredo da AWS).
   - O backend consome `OTEL_EXPORTER_OTLP_ENDPOINT` e opcionalmente `NEW_RELIC_LICENSE_KEY` / `DD_API_KEY`.
   - O backend expõe métricas e healthchecks nas rotas padronizadas: `GET /health` e `/metrics`.

---

## 2. Requisitos Mandatórios da Banca Cobertos pelo Track B

Conforme a especificação oficial (`docs/tech-challenge/13SOAT - Fase 3 - Tech Challenge.pdf`):

| Requisito do Tech Challenge | Componente / Localização | Entregável do Track B |
| :--- | :--- | :--- |
| **1. Integração Datadog ou New Relic** | `AutoReparos.API` & OTel Collector | Integração via OpenTelemetry OTLP com New Relic (ou Datadog) com License Key configurável. |
| **2. Métricas de Performance & Uptime** | `AutoReparos.API` | Latência das APIs (ASP.NET Core Instrumentation), healthchecks (`/health`, liveness/readiness). |
| **3. Logs Estruturados em JSON** | `AutoReparos.API` / Serilog ou Microsoft.Extensions.Logging | Logs JSON contendo correlação estrita entre requisições (`TraceId`, `SpanId`). |
| **4. Alertas Automatizados** | New Relic / Datadog / OTel | Alertas configurados para latência de APIs e falhas no processamento de OSs / integrações. |
| **5. Dashboards Mandatórios** | Grafana / New Relic / Datadog | **Dashboard 1:** Volume diário de OS.<br>**Dashboard 2:** Tempo médio por status (Diagnóstico, Execução, Finalização).<br>**Dashboard 3:** Erros e falhas nas integrações. |
| **6. Métrica de Falha em Integrações** | `NotificacaoService.cs` & Use Cases | Instrumentar contador `notificacoes.falhas` no `Meter` OTel com testes unitários e de integração. |
| **7. Documentação de Software & Banco** | `docs/architecture/` | **RFC 003** (Auth Serverless CPF+Email), **ADR 002** (Isolamento de Dados), **ADR 003** (Observabilidade), Justificativa formal do Banco PostgreSQL 16 + Modelo ER e Diagrama de Sequência. |
| **8. Roteiro do Vídeo & Minuta do PDF** | `docs/entrega/` | Roteiro estruturado para vídeo de até 15 minutos e minuta do documento PDF consolidado. |

---

## 3. Arquitetura Alvo de Observabilidade & Telemetria (Track B)

```mermaid
flowchart TD
    subgraph Client_Traffic ["Tráfego de Entrada"]
        ClientReq["Requisições HTTP (Portal / Gateway / Operadores)"]
    end

    subgraph App_Runtime ["AutoReparos Backend (.NET 10 API)"]
        HTTP_Layer["ASP.NET Core Minimal APIs & Controllers"]
        UseCases["Application Use Cases (OrdensServicos, Clientes)"]
        NotifService["NotificacaoService (SendGrid Integration)"]
        OTel_SDK["OpenTelemetry .NET SDK<br/>(ActivitySource & MeterProvider)"]
        JSON_Logger["Structured JSON Logger<br/>(TraceId & SpanId correlation)"]
    end

    subgraph Local_Collector ["Camada de Coleta (Local ou K8s)"]
        OTel_Collector["OpenTelemetry Collector<br/>(Recv: OTLP 4317 gRPC / 4318 HTTP)"]
    end

    subgraph Observability_Platform ["Plataforma de Observabilidade (New Relic / Datadog)"]
        NR_APM["APM: Latência de APIs, Throughput, Erros HTTP"]
        NR_Logs["Logs Explorer: JSON Search com TraceId"]
        NR_Metrics["Custom Metrics: notificacoes.falhas, volume diário"]
        NR_Dashboards["3 Dashboards Obrigatórios"]
        NR_Alerts["Políticas de Alerta: Falha de Integração & Latência p95"]
    end

    ClientReq --> HTTP_Layer
    HTTP_Layer --> UseCases
    UseCases --> NotifService

    HTTP_Layer -.->|Trace HTTP & Duration| OTel_SDK
    UseCases -.->|Business Spans| OTel_SDK
    NotifService -.->|Incrementa falhas| OTel_SDK
    HTTP_Layer -.->|Log context: TraceId, SpanId| JSON_Logger

    OTel_SDK -->|Export OTLP| OTel_Collector
    JSON_Logger -->|Stdout / OTLP Logs| OTel_Collector

    OTel_Collector -->|OTLP gRPC (API Key / License Key)| Observability_Platform
```

---

## 4. Contrato de Interface Técnica com o Track A

Para manter o trabalho 100% assíncrono:

1. **Recepção de OTLP:**
   - O código .NET 10 lê a variável `OTEL_EXPORTER_OTLP_ENDPOINT`. Se estiver vazia ou omitida, faz fallback gracioso para `http://localhost:4317` (Docker local) ou exportador de console em desenvolvimento.
2. **Logs Estruturados:**
   - Os logs são emitidos na saída padrão (`stdout`) em formato JSON compatível com o coletor de logs do Kubernetes (Fluentbit/DaemonSet) e CloudWatch.
3. **Healthcheck:**
   - O endpoint `GET /health` responde `200 OK` com payload `{"status":"Healthy"}` e checagem de conectividade com o banco de dados (Readiness).

---

## 5. Plano Passo a Passo de Execução (Track B)

### Passo 1: Configuração do OpenTelemetry SDK e Integração com New Relic / Datadog
**Diretório:** `AutoReparos.API` & `AutoReparos.Infra`  
**Branch de trabalho:** `feat/fase3-observability-otel-newrelic`  

1. **Pacotes NuGet Utilizados (.NET 10):**
   - `OpenTelemetry.Extensions.Hosting`
   - `OpenTelemetry.Instrumentation.AspNetCore`
   - `OpenTelemetry.Instrumentation.Http`
   - `OpenTelemetry.Instrumentation.EntityFrameworkCore`
   - `OpenTelemetry.Instrumentation.Runtime`
   - `OpenTelemetry.Exporter.OpenTelemetryProtocol`
2. **Configuração de Tracing & Metrics em [`DependencyInjectionAPI.cs`](../../../../submodules/AutoReparos.App/AutoReparos.API/DependencyInjectionAPI.cs):**
   - Registrar `Meter` de negócio: `AutoReparos.BusinessMetrics` (versão `"1.0.0"`).
   - Exportador OTLP configurado para ler endpoint via configuração:
     ```csharp
     var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317";
     var newRelicKey = configuration["NEW_RELIC_LICENSE_KEY"];

     builder.Services.AddOpenTelemetry()
         .WithTracing(tracing => tracing
             .AddSource("AutoReparos.API")
             .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
             .AddHttpClientInstrumentation()
             .AddEntityFrameworkCoreInstrumentation(opts => opts.SetDbStatementForText = true)
             .AddOtlpExporter(opt => {
                 opt.Endpoint = new Uri(otlpEndpoint);
                 if (!string.IsNullOrWhiteSpace(newRelicKey))
                     opt.Headers = $"api-key={newRelicKey}";
             }))
         .WithMetrics(metrics => metrics
             .AddMeter("AutoReparos.API")
             .AddMeter("AutoReparos.BusinessMetrics")
             .AddAspNetCoreInstrumentation()
             .AddHttpClientInstrumentation()
             .AddRuntimeInstrumentation()
             .AddOtlpExporter(opt => {
                 opt.Endpoint = new Uri(otlpEndpoint);
                 if (!string.IsNullOrWhiteSpace(newRelicKey))
                     opt.Headers = $"api-key={newRelicKey}";
             }));
     ```
3. **Configuração do OpenTelemetry Collector no `docker-compose.yml` e `infra/otel/otel-collector-config.yaml`:**
   - Adicionar pipeline exportando para o New Relic OTLP endpoint (`otlp.nr-data.net:4317`) ou Datadog.

---

### Passo 2: Logs Estruturados em JSON com Correlação (`TraceId` e `SpanId`)
**Arquivo:** `AutoReparos.API/Program.cs` e `appsettings.json`

1. **Formato JSON Padronizado:**
   - Configurar o logging do ASP.NET Core para utilizar JSON console formatter (`AddJsonConsole`) incluindo `Activity.Current?.TraceId` e `Activity.Current?.SpanId` nos scopes de cada requisição.
2. **Invariantes do Log:**
   - Cada log de erro, transação de OS ou autenticação deve carregar:
     - `timestamp` (ISO-8601 UTC)
     - `level` (`Information`, `Warning`, `Error`)
     - `message`
     - `traceId` (W3C Hex 32 chars)
     - `spanId` (W3C Hex 16 chars)
     - `correlationId` (extraído do header `X-Correlation-Id` ou gerado via GUID)

---

### Passo 3: Instrumentação de Métricas de Negócio & Tratamento de Falhas
**Arquivo:** `AutoReparos.Infra/Services/NotificacaoService.cs`

1. **Criação do Contador de Falhas em Integrações:**
   - Declarar e injetar o `Meter`:
     ```csharp
     public static class AutoReparosMetrics
     {
         public static readonly Meter Meter = new("AutoReparos.BusinessMetrics", "1.0.0");
         public static readonly Counter<long> NotificacoesFalhas = 
             Meter.CreateCounter<long>("notificacoes.falhas", description: "Contador de falhas no envio de notificações por canal e motivo");
         public static readonly Counter<long> OrdensServicoCriadas = 
             Meter.CreateCounter<long>("ordens_servico.criadas", description: "Volume de ordens de serviço criadas");
     }
     ```
2. **Atualização do `NotificacaoService.cs`:**
   - No bloco `catch (Exception ex)`, além de registrar o log estruturado em JSON com o `TraceId`, invocar:
     ```csharp
     AutoReparosMetrics.NotificacoesFalhas.Add(1, 
         new KeyValuePair<string, object?>("canal", "email"),
         new KeyValuePair<string, object?>("motivo", ex.GetType().Name));
     ```
3. **Testes Unitários Obrigatórios ([`NotificacaoServiceTests.cs`]):**
   - Criar teste com simulação de erro no client SendGrid validando que a exceção é tratada e a métrica `notificacoes.falhas` é devidamente incrementada em 1.

---

### Passo 4: Implementação dos 3 Dashboards Obrigatórios & Alertas
**Diretório:** `infra/grafana/dashboards/` ou `infra/observability/dashboards/`

Criar as definições em JSON dos três painéis exigidos pela especificação:

1. **Dashboard 1: Volume Diário de Ordens de Serviço (`dashboard_volume_diario.json`):**
   - Visualização: Gráfico de barras temporais (Time Series / Bar Chart).
   - Fonte de dados: Endpoint `/api/dashboard/metricas/volume-diario` (já implementado no backend) ou métrica Prometheus `ordens_servico_criadas_total`.
   - Agrupamento: Por dia (últimos 30 dias) e por status.
2. **Dashboard 2: Tempo Médio por Status (`dashboard_tempo_medio_status.json`):**
   - Visualização: Painel de Gauge / Estatísticas e Time Series.
   - Métricas:
     - Tempo Médio de **Diagnóstico** (`EnvioAprovacaoEm - DiagnosticoIniciadoEm`).
     - Tempo Médio de **Execução** (`FinalizadoEm - IniciadoEm`).
     - Tempo Médio **Total**.
   - Fonte de dados: Endpoint `/api/dashboard/metricas/tempo-medio` ou métricas OTel.
3. **Dashboard 3: Erros e Falhas nas Integrações (`dashboard_erros_integracoes.json`):**
   - Visualização: Gráfico de linha com picos de erro e tabela de incidentes.
   - Métrica: `rate(notificacoes_falhas[5m])`, contagem de falhas SendGrid, erros 5xx na API.
4. **Configuração dos Alertas Automatizados (`infra/observability/alerts.json`):**
   - **Alerta 1 (Falha de Integração):** Dispara quando `rate(notificacoes.falhas[1m]) > 0` por 3 minutos consecutivos.
   - **Alerta 2 (Latência de API):** Dispara quando `p95(http.server.duration) > 2000ms` por 5 minutos consecutivos.

---

### Passo 5: Suíte de Testes de Observabilidade e Resiliência
**Projetos:** `AutoReparos.Application.Tests` e `AutoReparos.IntegrationTests`

1. **Testes Unitários:**
   - Adicionar em `AutoReparos.Application.Tests` suíte `BusinessMetricsTests.cs` validando contadores e tags.
2. **Testes de Integração:**
   - Adicionar em `AutoReparos.IntegrationTests/Features/Observability/ObservabilityIntegrationTests.cs`:
     - Teste de `GET /health` respondendo 200 OK com banco de dados saudável.
     - Teste de correlação de logs: disparar requisição com header `X-Correlation-Id: test-correlation-123` e validar presença do header de resposta e atributos de contexto.

---

### Passo 6: Documentação de Arquitetura de Software & Dados (Track B)
**Diretório de Entrega:** `docs/architecture/`

Elaborar os documentos formais atribuídos ao software e modelagem:
1. **`RFC-003-serverless-client-authentication.md`**:
   - Racional da autenticação de clientes via combinação única CPF + E-mail.
   - Justificativa técnica para não poluir a tabela `AspNetUsers` com clientes finais (Zero Identity Pollution).
   - Ciclo de vida do JWT efêmero assinado (1h, HMAC-SHA256).
2. **`ADR-002-data-isolation-and-zero-trust-claims.md`**:
   - Decisão arquitetural do isolamento estrito de dados nas consultas do cliente (`Meus Veículos` e `Minhas OSs`).
   - Política Zero-Trust: cliente autenticado só enxerga dados atrelados ao seu próprio `ClienteId`/`CPF`. Consultas de placas alheias retornam lista vazia `[]` sem expor erro 404 ou 500.
3. **`ADR-003-end-to-end-observability-strategy.md`**:
   - Adoção de OpenTelemetry como padrão neutro de instrumentação (Vendor-Agnostic) integrado ao New Relic / Datadog.
   - Racional do OTel Collector intermediando tráfego sem onerar a aplicação com SDKs proprietários.
4. **`database-selection-and-data-model.md` (Justificativa Formal do Banco):**
   - Justificativa formal da escolha do **PostgreSQL 16**:
     - Conformidade estrita com ACID (Atomicidade, Consistência, Isolamento, Durabilidade) essencial para finanças e estoque da oficina.
     - Suporte a constraints de integridade referencial (veículos pertencem a clientes, OSs pertencem a veículos).
     - Desempenho com índices B-Tree e conexões concorrentes via PgBouncer / RDS Connection Pool.
     - Comparação explícita: Por que não MongoDB (falta de integridade referencial transacional entre OS e Insumos)? Por que não MySQL (PostgreSQL oferece recursos avançados de JSONB, tipos customizados e performance superior em agregações)?
   - **Modelo Entidade-Relacionamento (ER) Atualizado:**
     - Diagrama Mermaid ER completo contendo todas as tabelas (`Clientes`, `Veiculos`, `OrdensServicos`, `Servicos`, `Insumos`, `OrdemServicoItens`, `AspNetUsers`).
     - Inclusão dos novos atributos da Fase 3: `Cliente.InativoEm`, `OrdemServico.DiagnosticoIniciadoEm`.
     - Dicionário de dados completo (tipos de dados, PKs, FKs, nullability, índices).
5. **Diagrama de Sequência Detalhado:**
   - Diagrama Mermaid cobrindo o fluxo completo:
     - Cliente acessa portal -> POST /auth/cliente no API Gateway -> Acionamento Lambda -> SELECT no RDS -> Retorno JWT efêmero -> Requisição subsequente GET /api/clientes/meus-veiculos com Bearer Token -> Roteamento para K8s -> Validação JWT no backend -> Resposta HTTP 200.

---

### Passo 7: Roteiro do Vídeo Demonstrativo & Estrutura do PDF de Entrega
**Diretório:** `docs/entrega/`

1. **`roteiro_video_demonstracao_15min.md`**:
   - Roteiro estruturado cronometrado para gravação de até 15 minutos (YouTube/Vimeo não listado), cobrindo exatamente os 6 itens da banca:
     - Minuto 00:00 - 02:00: Apresentação da equipe, contexto da oficina e topologia de 4 repositórios.
     - Minuto 02:00 - 05:00: Demonstração da execução da pipeline CI/CD no GitHub Actions (build, testes, plan, deploy).
     - Minuto 05:00 - 08:00: Demonstração ao vivo da autenticação Serverless com CPF + E-mail e emissão de JWT.
     - Minuto 08:00 - 10:30: Consumo das APIs protegidas (`/meus-veiculos`, `/minhas-os`) e isolamento Zero-Trust.
     - Minuto 10:30 - 13:30: Demonstração ao vivo dos Dashboards de monitoramento (Volume diário, Tempos médios, Erros).
     - Minuto 13:30 - 15:00: Apresentação de logs estruturados em JSON, traces correlacionados no New Relic/Jaeger e encerramento.
2. **`template_entrega_portal_fiap.md` (Minuta para Geração do PDF Final):**
   - Seção 1: Identificação do Grupo e Curso.
   - Seção 2: Tabela de Links dos 4 Repositórios Git (`AutoReparos.App`, `AutoReparos.AuthLambda`, `AutoReparos.Infra.Database`, `AutoReparos.Infra.K8s`).
   - Seção 3: Link do Vídeo Demonstrativo (YouTube/Vimeo).
   - Seção 4: Links para Documentação Técnica (RFCs, ADRs, Diagramas, Swagger/Postman).
   - Seção 5: Evidência de Convite do usuário `soat-architecture` adicionado a todos os 4 repositórios.

---

## 6. Critérios de Aceite & Validação Empírica (Definition of Done - DoD)

Antes de considerar o Track B concluído, execute e valide os seguintes comandos:

```bash
# 1. Compilação da Solution
dotnet build AutoReparos.slnx

# 2. Execução de Todos os Testes Unitários de Aplicação e Domínio
dotnet test AutoReparos.Domain.Tests/AutoReparos.Domain.Tests.csproj
dotnet test AutoReparos.Application.Tests/AutoReparos.Application.Tests.csproj

# 3. Execução dos Testes de Integração com Banco Real (Testcontainers PostgreSQL)
dotnet test AutoReparos.IntegrationTests/AutoReparos.IntegrationTests.csproj
# Deve validar: 62+/62+ testes aprovados com 100% de sucesso

# 4. Execução da Stack Local de Observabilidade
docker-compose up -d --build
# Validar se /health responde HTTP 200:
curl -i http://localhost:8080/health

# 5. Validação dos Endpoints de Métricas do Dashboard
curl -i http://localhost:8080/api/dashboard/metricas/volume-diario
curl -i http://localhost:8080/api/dashboard/metricas/tempo-medio
```

---

## 7. Checklist de Entregáveis Finais do Track B

- [ ] OpenTelemetry SDK configurado para exportação OTLP no backend .NET 10.
- [ ] Logs estruturados em JSON com correlação `TraceId` e `SpanId`.
- [ ] Métrica `notificacoes.falhas` instrumentada em `NotificacaoService.cs` com testes unitários.
- [ ] 3 Dashboards obrigatórios definidos em JSON (Volume Diário, Tempos Médios, Falhas de Integração).
- [ ] Políticas de alerta configuradas para latência e falhas de processamento.
- [ ] Documentos `RFC-003`, `ADR-002`, `ADR-003` finalizados em `docs/architecture/`.
- [ ] Documento de Justificativa Formal do Banco de Dados PostgreSQL 16 com Diagrama ER e Dicionário de Dados.
- [ ] Diagrama de Sequência detalhado (Auth CPF -> Lambda -> RDS -> Backend K8s).
- [ ] Roteiro do vídeo demonstrativo de até 15 minutos (`roteiro_video_demonstracao_15min.md`).
- [ ] Minuta do PDF de entrega final para o Portal FIAP (`template_entrega_portal_fiap.md`).
