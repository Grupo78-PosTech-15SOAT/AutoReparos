using AutoReparos.Application.Servicos.DTOs.Response;

namespace AutoReparos.Application.Servicos.UseCases.Interfaces
{
    public interface IObterTempoMedioServicosUseCase
    {
        Task<IEnumerable<TempoMedioServicoDto>> ExecuteAsync();
    }
}
