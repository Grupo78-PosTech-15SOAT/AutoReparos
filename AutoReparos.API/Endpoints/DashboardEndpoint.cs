using AutoReparos.API.Controllers;
using AutoReparos.Application.Dashboard.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AutoReparos.API.Endpoints
{
    public static class DashboardEndpoint
    {
        public static void MapDashboardEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/dashboard")
                .WithTags("Dashboard")
                .RequireAuthorization("OperadorOficina");

            group.MapGet("/metrics", (DashboardController controller) => controller.GetMetrics())
                .WithName("GetDashboardMetrics")
                .WithSummary("Retorna os indicadores consolidados do dashboard")
                .Produces<DashboardMetricsDto>(StatusCodes.Status200OK);

            group.MapGet("/volume-diario", (DashboardController controller, [FromQuery] int? dias) => controller.GetVolumeDiario(dias))
                .WithName("GetDashboardVolumeDiario")
                .WithSummary("Retorna o volume diário de ordens de serviço criadas e finalizadas")
                .Produces<IEnumerable<DashboardVolumeDiarioDto>>(StatusCodes.Status200OK);

            group.MapGet("/tempos-medios", (DashboardController controller) => controller.GetTemposMedios())
                .WithName("GetDashboardTemposMedios")
                .WithSummary("Retorna os tempos médios por status (diagnóstico, execução, finalização)")
                .Produces<DashboardTempoMedioStatusDto>(StatusCodes.Status200OK);
        }
    }
}
