using AutoReparos.Application.Servicos.DTOs.Response;

namespace AutoReparos.Application.Servicos.UseCases.Interfaces
{
    public interface IObterTempoMedioServicoPorIdUseCase
    {
        Task<TempoMedioServicoDto?> ExecuteAsync(Guid id);
    }
}
