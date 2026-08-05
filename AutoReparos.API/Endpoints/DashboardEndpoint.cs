using AutoReparos.Application.Dashboard.DTOs;
using AutoReparos.Application.Dashboard.Services;

namespace AutoReparos.API.Endpoints
{
    public static class DashboardEndpoint
    {
        public static void MapDashboardEndpoints(this WebApplication app)
        {
            app.MapGet("/api/dashboard/metrics", (IDashboardQueryService dashboardService) => dashboardService.GetMetricsAsync())
                .WithTags("Dashboard")
                .WithName("GetDashboardMetrics")
                .WithSummary("Retorna os indicadores consolidados do dashboard")
                .RequireAuthorization()
                .Produces<DashboardMetricsDto>(StatusCodes.Status200OK);
        }
    }
}
