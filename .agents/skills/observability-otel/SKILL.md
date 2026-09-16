---
name: observability-otel
description: Use this skill when configuring, monitoring, debugging, or adding metrics/traces via OpenTelemetry, Jaeger, New Relic, Prometheus, Grafana, and Loki in AutoReparos.
---

# OpenTelemetry & Observability Skill Guide

This skill provides step-by-step procedures for telemetry instrumentations (traces, metrics, logs) in .NET 10 (AutoReparos) and workflows for viewing data across Jaeger, Prometheus, Grafana, Loki, and configuring New Relic OTLP export via OpenTelemetry Collector.

---

## Architectural Overview & Infrastructure Topology

AutoReparos uses an OpenTelemetry-based observability architecture with a centralized OpenTelemetry Collector routing telemetry to specialized backends:

- **Telemetry Exporter (`AutoReparos.API`)**: Exports Traces, Metrics, and Logs via OTLP gRPC (`http://otel-collector:4317`) configured in `OpenTelemetryExtensions.cs` ([`OpenTelemetryExtensions.cs`](../../../AutoReparos.API/OpenTelemetryExtensions.cs)).
- **OpenTelemetry Collector (`autoreparos-otel-collector`)**: Defined in `docker-compose.yml` ([`docker-compose.yml`](../../../docker-compose.yml)) and configured in `infra/otel/otel-collector-config.yaml` ([`otel-collector-config.yaml`](../../../infra/otel/otel-collector-config.yaml)). Receives OTLP data and routes to Jaeger, Prometheus, Loki, and optionally New Relic.
- **Jaeger UI (`autoreparos-jaeger`)**: http://localhost:16686 (Distributed Traces)
- **Prometheus (`autoreparos-prometheus`)**: http://localhost:9090 (Metrics Engine)
- **Loki (`autoreparos-loki`)**: http://localhost:3100 (Log Aggregator)
- **Grafana (`autoreparos-grafana`)**: http://localhost:3000 (Unified Dashboards for Prometheus & Loki)

---

## 1. Step-by-Step Procedure: Custom ActivitySource Tracing in C# .NET 10

Use `System.Diagnostics.ActivitySource` to capture custom span operations in business services or use cases.

### Step 1: Define or Inject ActivitySource
Create a static or injected `ActivitySource` matching the registered source name in `OpenTelemetryExtensions.cs` ([`OpenTelemetryExtensions.cs`](../../../AutoReparos.API/OpenTelemetryExtensions.cs)).

```csharp
using System.Diagnostics;

namespace AutoReparos.Application.OrdemServico.UseCases;

public class AprovacaoOrdemServicoUseCase
{
    private static readonly ActivitySource ActivitySource = new("AutoReparos.API");

    public async Task ExecuteAsync(Guid osId)
    {
        using var activity = ActivitySource.StartActivity("OrdemServico.AprovacaoProcess");
        activity?.SetTag("ordem_servico.id", osId.ToString());
        activity?.SetTag("component", "UseCase");

        try
        {
            // Business execution
            await ProcessAprovacaoInternalAsync(osId);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }

    private Task ProcessAprovacaoInternalAsync(Guid osId) => Task.CompletedTask;
}
```

### Step 2: Register Activity Source Name in OpenTelemetry Tracing
Ensure the `ActivitySource` name is registered in `WithTracing` inside `OpenTelemetryExtensions.cs` ([`OpenTelemetryExtensions.cs`](../../../AutoReparos.API/OpenTelemetryExtensions.cs)):

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .ConfigureResource(r => r.AddService(serviceName))
            .AddSource(serviceName) // Must match "AutoReparos.API" or custom source names
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter(options => options.Endpoint = new Uri(otelEndpoint));
    });
```

---

## 2. Step-by-Step Procedure: Custom Prometheus Counters & Histograms in C# .NET 10

Use `System.Diagnostics.Metrics` to emit custom application metrics (Counters, Histograms, Gauges).

### Step 1: Declare Meter and Instrument Counters/Histograms
Define business instruments inside your service or domain metrics class:

```csharp
using System.Diagnostics.Metrics;

namespace AutoReparos.Application.Metrics;

public static class ApplicationMetrics
{
    public const string MeterName = "AutoReparos.API";
    private static readonly Meter Meter = new(MeterName, "1.0.0");

    // Counter for tracking total executed service orders
    public static readonly Counter<long> OrdemServicoCriadasCounter = Meter.CreateCounter<long>(
        name: "autoreparos_ordem_servico_criadas_total",
        unit: "{ordens}",
        description: "Total de Ordens de Serviço criadas no sistema");

    // Histogram for execution duration measurement
    public static readonly Histogram<double> ProcessamentoDuracaoHistogram = Meter.CreateHistogram<double>(
        name: "autoreparos_processamento_duracao_seconds",
        unit: "s",
        description: "Duração do processamento de regras de negócios em segundos");
}
```

### Step 2: Record Metric Measurements in Business Logic
```csharp
public async Task CriarOrdemServicoAsync(CriarOrdemServicoDto dto)
{
    var stopwatch = Stopwatch.StartNew();

    // Business Logic Execution
    await _repository.AddAsync(...);

    stopwatch.Stop();

    // Record metrics
    ApplicationMetrics.OrdemServicoCriadasCounter.Add(1, 
        new KeyValuePair<string, object?>("status", "Criada"),
        new KeyValuePair<string, object?>("tipo", dto.TipoServico));

    ApplicationMetrics.ProcessamentoDuracaoHistogram.Record(
        stopwatch.Elapsed.TotalSeconds,
        new KeyValuePair<string, object?>("operacao", "CriarOrdemServico"));
}
```

### Step 3: Register Meter in `OpenTelemetryExtensions.cs`
Register `MeterName` in `WithMetrics` configuration ([`OpenTelemetryExtensions.cs`](../../../AutoReparos.API/OpenTelemetryExtensions.cs)):

```csharp
services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .ConfigureResource(r => r.AddService(serviceName))
            .AddMeter(ApplicationMetrics.MeterName) // Register custom meter
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(options => options.Endpoint = new Uri(otelEndpoint));
    });
```

---

## 3. Step-by-Step Procedure: Inspect Distributed Traces in Jaeger UI

Jaeger displays end-to-end distributed trace context (HTTP requests -> Application Use Cases -> Database queries).

1. **Access Jaeger UI**:
   Open browser at [http://localhost:16686](http://localhost:16686).
2. **Select Service**:
   In the left sidebar, select **Service** -> `AutoReparos.API`.
3. **Filter Spans / Traces**:
   - **Operation**: Select endpoint/span (e.g. `POST /api/ordemservico` or `OrdemServico.AprovacaoProcess`).
   - **Tags**: Search by specific attributes e.g. `ordem_servico.id=abc-123` or `http.status_code=500`.
   - **Limit Results**: Adjust to last 20 traces or specify time window (e.g., Last 1 Hour).
4. **Analyze Trace Timeline**:
   - Click **Find Traces** and select a trace entry.
   - Inspect span duration waterfall to identify latency bottlenecks (e.g. slow EF Core SQL queries vs external HTTP calls).
   - Click on individual span details to read tags, logs, and exception stack traces.

---

## 4. Step-by-Step Procedure: Query Prometheus Metrics

Prometheus collects metrics exposed by OpenTelemetry Collector at `http://otel-collector:8889`.

1. **Access Prometheus UI**:
   Open browser at [http://localhost:9090](http://localhost:9090).
2. **Execute PromQL Queries**:
   Navigate to the **Graph** tab and query standard or custom metrics:
   - **Custom Counter Rate**:
     ```promql
     rate(autoreparos_ordem_servico_criadas_total[5m])
     ```
   - **Histogram 95th Percentile Latency**:
     ```promql
     histogram_quantile(0.95, sum(rate(autoreparos_processamento_duracao_seconds_bucket[5m])) by (le))
     ```
   - **ASP.NET Core Active Requests**:
     ```promql
     http_server_current_requests{http_service_name="AutoReparos.API"}
     ```
3. **Inspect Targets Health**:
   Go to **Status** -> **Targets** to confirm `otel-collector` endpoint (`http://otel-collector:8889/metrics`) is `UP`.

---

## 5. Step-by-Step Procedure: Query Loki Logs in Grafana

Loki aggregates logs sent via OTLP gRPC through OpenTelemetry Collector.

1. **Access Grafana**:
   Open browser at [http://localhost:3000](http://localhost:3000) (Credentials: `admin` / `admin`).
2. **Navigate to Explore**:
   Click on **Explore** (compass icon) on the left sidebar.
3. **Select Loki Data Source**:
   Choose **Loki** from the dropdown datasource selector.
4. **Execute LogQL Queries**:
   - **Filter Logs by Service**:
     ```logql
     {service_name="AutoReparos.API"}
     ```
   - **Search Error Logs**:
     ```logql
     {service_name="AutoReparos.API"} |= "Exception" |~ "(?i)error"
     ```
   - **Format JSON / Extract Fields**:
     ```logql
     {service_name="AutoReparos.API"} | json | line_format "{{.Body}}"
     ```
5. **Correlate Logs with Trace IDs**:
   Extract `trace_id` from log line details and search the trace directly in Jaeger UI (http://localhost:16686).

---

## 6. Step-by-Step Procedure: Configure New Relic OTLP Exporter in OpenTelemetry Collector

To export traces, metrics, and logs to New Relic using native OTLP without installing proprietary agents:

### Step 1: Obtain New Relic License Key & Endpoint
- Region **US**: `otlp.nr-data.net:4317`
- Region **EU**: `otlp.eu01.nr-data.net:4317`
- Obtain your **New Relic Ingest License Key** from your New Relic account portal.

### Step 2: Update OpenTelemetry Collector Config
Modify `infra/otel/otel-collector-config.yaml` ([`otel-collector-config.yaml`](../../../infra/otel/otel-collector-config.yaml)):

```yaml
exporters:
  prometheus:
    endpoint: 0.0.0.0:8889
    namespace: "autoreparos"

  otlp/jaeger:
    endpoint: jaeger:4317
    tls:
      insecure: true

  loki:
    endpoint: http://loki:3100/loki/api/v1/push

  # New Relic OTLP Exporter Configuration
  otlp/newrelic:
    endpoint: otlp.nr-data.net:4317 # Use otlp.eu01.nr-data.net:4317 for EU
    headers:
      api-key: "${NEW_RELIC_LICENSE_KEY}"

service:
  extensions: [health_check]
  pipelines:
    traces:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [otlp/jaeger, otlp/newrelic]
    metrics:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [prometheus, otlp/newrelic]
    logs:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [loki, otlp/newrelic]
```

### Step 3: Pass Environment Variable in Docker Compose
Update `docker-compose.yml` ([`docker-compose.yml`](../../../docker-compose.yml)) under `otel-collector`:

```yaml
  otel-collector:
    image: otel/opentelemetry-collector-contrib:0.96.0
    container_name: autoreparos-otel-collector
    restart: always
    environment:
      - NEW_RELIC_LICENSE_KEY=${NEW_RELIC_LICENSE_KEY}
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./infra/otel/otel-collector-config.yaml:/etc/otel-collector-config.yaml
```

---

## 7. Local Stack Startup & Verification

Verify the observability stack locally:

```bash
# Start all observability services and API
docker compose up -d

# Check OTel Collector logs
docker compose logs -f otel-collector
```

Access endpoints:
- **Jaeger UI**: http://localhost:16686
- **Prometheus UI**: http://localhost:9090
- **Loki / Grafana UI**: http://localhost:3000
- **OTel Collector Health Check**: http://localhost:13133
