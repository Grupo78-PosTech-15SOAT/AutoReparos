using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AutoReparos.API
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddOpenTelemetryObservability(
            this IServiceCollection services,
            IConfiguration configuration,
            ILoggingBuilder loggingBuilder)
        {
            var otelEndpoint = configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";
            var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "AutoReparos.API";

            var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName);

            services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing
                        .ConfigureResource(r => r.AddService(serviceName))
                        .AddSource(serviceName)
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                        })
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otelEndpoint);
                        });
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .ConfigureResource(r => r.AddService(serviceName))
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otelEndpoint);
                        });
                });

            loggingBuilder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(otelEndpoint);
                });
            });

            return services;
        }
    }
}
