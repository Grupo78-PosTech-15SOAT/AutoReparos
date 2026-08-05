using AutoReparos.Application.Dashboard.DTOs;

namespace AutoReparos.Application.Dashboard.Services
{
    public interface IDashboardQueryService
    {
        Task<DashboardMetricsDto> GetMetricsAsync();
    }
}
