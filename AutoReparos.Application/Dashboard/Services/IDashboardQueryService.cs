using AutoReparos.Application.Dashboard.DTOs;

namespace AutoReparos.Application.Dashboard.Services
{
    public interface IDashboardQueryService
    {
        Task<DashboardMetricsDto> GetMetricsAsync();
        Task<IEnumerable<DashboardVolumeDiarioDto>> GetVolumeDiarioAsync(int dias = 30);
        Task<DashboardTempoMedioStatusDto> GetTemposMediosStatusAsync();
    }
}
