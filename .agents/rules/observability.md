# OpenTelemetry & Full-Stack Observability Guidelines

This document defines mandatory guidelines and best practices for OpenTelemetry (OTel) instrumentation, distributed tracing (Jaeger), metrics collection (Prometheus), structured log correlation (Loki), dashboard provisioning (Grafana), and cloud backend exports (New Relic) within the `AutoReparos` solution.

---

## 1. Relevant Architecture & Code References

- **API OTel Registration**: [`AutoReparos.API/OpenTelemetryExtensions.cs`](file:///mnt/c/Code/AutoReparos/AutoReparos.API/OpenTelemetryExtensions.cs)
- **API Entrypoint**: [`AutoReparos.API/Program.cs`](file:///mnt/c/Code/AutoReparos/AutoReparos.API/Program.cs)
- **Container Infrastructure**: [`docker-compose.yml`](file:///mnt/c/Code/AutoReparos/docker-compose.yml)
- **OTel Collector Configuration**: [`infra/otel/otel-collector-config.yaml`](file:///mnt/c/Code/AutoReparos/infra/otel/otel-collector-config.yaml)
- **Prometheus Scraper Config**: [`infra/otel/prometheus.yml`](file:///mnt/c/Code/AutoReparos/infra/otel/prometheus.yml)
- **Loki Log Config**: [`infra/otel/loki-config.yaml`](file:///mnt/c/Code/AutoReparos/infra/otel/loki-config.yaml)
- **Grafana Datasources**: [`infra/grafana/provisioning/datasources/datasources.yaml`](file:///mnt/c/Code/AutoReparos/infra/grafana/provisioning/datasources/datasources.yaml)
- **Grafana Provisioned Dashboards**: [`infra/grafana/provisioning/dashboards/dashboards.yaml`](file:///mnt/c/Code/AutoReparos/infra/grafana/provisioning/dashboards/dashboards.yaml)

---

## 2. OpenTelemetry (OTel) Instrumentation in C# .NET 10

### 2.1 Core Registration & Configuration
Instrumentation MUST be registered in [`AutoReparos.API/OpenTelemetryExtensions.cs`](file:///mnt/c/Code/AutoReparos/AutoReparos.API/OpenTelemetryExtensions.cs) using standard OpenTelemetry SDK extensions for .NET 10.

- **Resource Builder**: Always set `service.name`, `service.version`, and `deployment.environment`.
- **Automatic Instrumentation**:
  - `AddAspNetCoreInstrumentation()` (capturing HTTP requests & unhandled exceptions).
  - `AddHttpClientInstrumentation()` (outgoing API calls).
  - `AddEntityFrameworkCoreInstrumentation()` (SQL query execution spans).
  - `AddRuntimeInstrumentation()` (CLR garbage collection, thread pool, memory).

```csharp
// GOOD: Standard AddOpenTelemetryObservability extension method
public static IServiceCollection AddOpenTelemetryObservability(
    this IServiceCollection services,
    IConfiguration configuration,
    ILoggingBuilder loggingBuilder)
{
    var otelEndpoint = configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";
    var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "AutoReparos.API";

    services.AddOpenTelemetry()
        .WithTracing(tracing =>
        {
            tracing
                .ConfigureResource(r => r.AddService(serviceName))
                .AddSource(serviceName)
                .AddSource("AutoReparos.Domain")
                .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = new Uri(otelEndpoint));
        })
        .WithMetrics(metrics =>
        {
            metrics
                .ConfigureResource(r => r.AddService(serviceName))
                .AddMeter("AutoReparos.API")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = new Uri(otelEndpoint));
        });

LoggingBuilder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
        options.AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(otelEndpoint));
    });

    return services;
}
```

### 2.2 Custom Spans with `ActivitySource`
Use static `ActivitySource` instances for explicit custom tracing within Application and Domain use cases.

```csharp
// GOOD: Creating custom spans for business operations
public static class DiagnosticConfig
{
    public static readonly ActivitySource Source = new("AutoReparos.API", "1.0.0");
}

public class AprovarOrdemServicoUseCase(IOrdemServicoRepository repository)
{
    public async Task ExecuteAsync(Guid osId, CancellationToken cancellationToken)
    {
        using var activity = DiagnosticConfig.Source.StartActivity("OrdemServico.Aprovar");
        activity?.SetTag("autoreparos.ordem_servico.id", osId.ToString());

        try
        {
            var os = await repository.GetByIdAsync(osId, cancellationToken);
            os.Aprovar();
            await repository.UpdateAsync(os, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
```

### 2.3 Custom Metrics with `Meter`
Use `System.Diagnostics.Metrics.Meter` to measure domain counters, execution durations, and business KPIs.

```csharp
// GOOD: Creating custom metrics for domain operations
public static class DiagnosticsMetrics
{
    public static readonly Meter Meter = new("AutoReparos.API", "1.0.0");

    public static readonly Counter<long> OrdensServicoCriadas = 
        Meter.CreateCounter<long>("autoreparos_ordens_servico_criadas_total", "count", "Total de OSs criadas");

    public static readonly Histogram<double> TempoAprovarOrdemServico = 
        Meter.CreateHistogram<double>("autoreparos_ordem_servico_aprovação_duration_seconds", "s", "Tempo de aprovação da OS");
}
```

### 2.4 Log Scopes & Structured Telemetry Correlation
- Enable `IncludeScopes = true` in OTel logging options.
- Inject contextual attributes using `ILogger.BeginScope` or structured placeholder tags.
- **NEVER** use string interpolation (`$"..."`) in logging methods. Always use structured message templates.

```csharp
// GOOD: Structured logging with scope correlation
using (logger.BeginScope(new Dictionary<string, object> { ["ClienteId"] = clienteId }))
{
    logger.LogInformation("Processando aprovação para Ordem de Serviço {OrdemServicoId}", osId);
}
```

---

## 3. OTLP Export Rules (gRPC 4317 & HTTP 4318)

### 3.1 Endpoint & Protocol Conventions
- **gRPC Export (Port 4317)**: Default protocol for high-throughput service-to-collector communication within Docker container network (`http://otel-collector:4317`).
- **HTTP Export (Port 4318)**: Alternative for edge services, browser clients, or firewalled infrastructure (`http://otel-collector:4318/v1/traces`).
- Configuration variables in [`docker-compose.yml`](file:///mnt/c/Code/AutoReparos/docker-compose.yml):
  ```yaml
  OpenTelemetry__Endpoint=http://otel-collector:4317
  OpenTelemetry__ServiceName=AutoReparos.API
  ```

### 3.2 OTel Collector Configuration Guidelines
The OpenTelemetry Collector defined in [`infra/otel/otel-collector-config.yaml`](file:///mnt/c/Code/AutoReparos/infra/otel/otel-collector-config.yaml) MUST maintain standard pipelines for `traces`, `metrics`, and `logs`.

```yaml
# GOOD: OTel Collector pipeline configuration
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:
    send_batch_size: 8192
    timeout: 1s
  memory_limiter:
    check_interval: 1s
    limit_percentage: 75
    spike_limit_percentage: 20

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

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [otlp/jaeger]
    metrics:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [prometheus]
    logs:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [loki]
```

---

## 4. Jaeger Distributed Tracing Guidelines

### 4.1 W3C Trace Context Propagation
- Outgoing HTTP calls via `HttpClientFactory` automatically attach `traceparent` and `tracestate` headers.
- Cross-service requests MUST preserve parent-child span relationships.

### 4.2 Span Tagging & Semantic Conventions
Follow OpenTelemetry Semantic Conventions for attribute keys:

| Category | Recommended Tag Key | Example Value |
| :--- | :--- | :--- |
| HTTP Method | `http.request.method` / `http.method` | `POST` |
| HTTP Route | `http.route` | `/api/v1/ordens-servico/{id}/aprovar` |
| HTTP Status | `http.response.status_code` | `200` |
| DB System | `db.system` | `postgresql` |
| DB Statement | `db.statement` | `SELECT * FROM "OrdensServico" WHERE "Id" = @p0` |
| Domain OS ID | `autoreparos.ordem_servico.id` | `a1b2c3d4-...` |
| Domain Cliente ID | `autoreparos.cliente.id` | `e5f6g7h8-...` |

### 4.3 Jaeger UI Inspection
- Access Jaeger UI at `http://localhost:16686` as configured in [`docker-compose.yml`](file:///mnt/c/Code/AutoReparos/docker-compose.yml).
- Search traces by `service_name=AutoReparos.API` or operation name.

---

## 5. Prometheus Metrics Guidelines

### 5.1 Metric Types & Usage

1. **Counters (`Counter<T>`)**:
   - Monotonically increasing counters for business events or request counts.
   - Example: `autoreparos_ordens_servico_criadas_total`.
2. **Histograms (`Histogram<T>`)**:
   - Measures duration, latency distributions, or payload sizes.
   - Example: `autoreparos_ordem_servico_processamento_duration_seconds`.
3. **Gauges / Observable Gauges (`ObservableGauge<T>`)**:
   - Instantaneous values that fluctuate up and down (e.g., active database connections, current stock levels).

### 5.2 Cardinality Rules
- **NEVER** use dynamic identifiers (GUIDs, CPF/CNPJ, User IDs, dynamic query strings) as metric label values.
- Low-cardinality labels allowed: `status`, `tipo_servico`, `environment`.

### 5.3 Prometheus Scraper Configuration
In [`infra/otel/prometheus.yml`](file:///mnt/c/Code/AutoReparos/infra/otel/prometheus.yml), scrape targets are pointed at the OTel Collector metrics exporter on port `8889`.

```yaml
global:
  scrape_interval: 5s
  evaluation_interval: 5s

scrape_configs:
  - job_name: 'otel-collector'
    static_configs:
      - targets: ['otel-collector:8889']
```

---

## 6. Loki Structured Log Correlation

### 6.1 Trace ID & Span ID Auto-Injection
- OpenTelemetry log provider extracts `Activity.Current.TraceId` and `Activity.Current.SpanId` automatically.
- Logs pushed to Loki via OTel Collector (`http://loki:3100/loki/api/v1/push`) inherit trace context.

### 6.2 LogQL Querying & Trace Search in Grafana
- Search logs by service label: `{exporter="OTLP"} |= "AutoReparos.API"`
- Filter logs for specific trace ID: `{service_name="AutoReparos.API"} | json | trace_id = "4bf92f3577b34da6a3ce929d0e0e4736"`

---

## 7. Grafana Dashboard Provisioning Guidelines

### 7.1 Datasource Setup
Datasources are automatically provisioned in [`infra/grafana/provisioning/datasources/datasources.yaml`](file:///mnt/c/Code/AutoReparos/infra/grafana/provisioning/datasources/datasources.yaml) with explicit service URLs:
- **Prometheus**: `http://prometheus:9090`
- **Jaeger**: `http://jaeger:16686`
- **Loki**: `http://loki:3100`

### 7.2 Dashboard Provisioning Path
Dashboards MUST be registered via [`infra/grafana/provisioning/dashboards/dashboards.yaml`](file:///mnt/c/Code/AutoReparos/infra/grafana/provisioning/dashboards/dashboards.yaml) pointing to JSON definitions in [`infra/grafana/provisioning/dashboards/json/`](file:///mnt/c/Code/AutoReparos/infra/grafana/provisioning/dashboards/json/dotnet_api_overview.json).

### 7.3 Key Dashboard Metrics (Golden Signals)
Every production dashboard must present:
1. **Latency**: Request duration P95/P99 (`histogram_quantile(0.95, sum(rate(http_server_duration_milliseconds_bucket[5m])) by (le))`).
2. **Traffic**: Request rate per second (`sum(rate(http_server_requests_received_total[1m]))`).
3. **Errors**: HTTP 5xx vs 2xx/4xx error rate ratio.
4. **Saturation**: Memory working set, CPU utilization, thread pool queue length.

---

## 8. New Relic OTLP Export Integration Guidelines

### 8.1 Integration Architecture
To export telemetry data to New Relic, configure an OTLP exporter in [`infra/otel/otel-collector-config.yaml`](file:///mnt/c/Code/AutoReparos/infra/otel/otel-collector-config.yaml) or directly in .NET API via `OtlpExporterOptions`.

### 8.2 Endpoint & Header Specification

| Region | OTLP gRPC Endpoint | OTLP HTTP Endpoint |
| :--- | :--- | :--- |
| **US Data Center** | `otlp.nr-data.net:4317` | `https://otlp.nr-data.net/v1/traces` |
| **EU Data Center** | `otlp.eu01.nr-data.net:4317` | `https://otlp.eu01.nr-data.net/v1/traces` |

### 8.3 Collector Exporter Configuration for New Relic
Add a dedicated exporter block in [`infra/otel/otel-collector-config.yaml`](file:///mnt/c/Code/AutoReparos/infra/otel/otel-collector-config.yaml):

```yaml
exporters:
  otlp/newrelic:
    endpoint: otlp.nr-data.net:4317
    headers:
      api-key: ${NEW_RELIC_LICENSE_KEY}

service:
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

---

## 9. Verification & Health Checks

Before completing observability changes or PRs, run the following verification steps:

```bash
# 1. Run local container stack
docker-compose up -d --build

# 2. Check OTel Collector health check status (Port 13133)
curl http://localhost:13133/

# 3. Access Observability UIs
# - Grafana:    http://localhost:3000 (admin/admin)
# - Jaeger:     http://localhost:16686
# - Prometheus: http://localhost:9090
# - Loki:       http://localhost:3100
```
