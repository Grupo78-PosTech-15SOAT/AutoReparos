using AutoReparos.Application.Dashboard.DTOs;
using AutoReparos.Application.Dashboard.Services;
using Microsoft.AspNetCore.Http;

namespace AutoReparos.API.Controllers
{
    public class DashboardController(IDashboardQueryService dashboardService)
    {
        public async Task<IResult> GetMetrics()
        {
            var metrics = await dashboardService.GetMetricsAsync();
            return Results.Ok(metrics);
        }

        public async Task<IResult> GetVolumeDiario(int? dias)
        {
            var volume = await dashboardService.GetVolumeDiarioAsync(dias ?? 30);
            return Results.Ok(volume);
        }

        public async Task<IResult> GetTemposMedios()
        {
            var tempos = await dashboardService.GetTemposMediosStatusAsync();
            return Results.Ok(tempos);
        }
    }
}
