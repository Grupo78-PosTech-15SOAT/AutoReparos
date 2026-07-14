using AutoReparos.Application.OrdensServicos.DTOs.Response;

namespace AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces
{
    public interface IObterOrdemServicoPorIdUseCase
    {
        Task<OrdemServicoDetalheDto?> ExecuteAsync(Guid id);
    }
}
