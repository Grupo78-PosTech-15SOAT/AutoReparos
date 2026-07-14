using AutoReparos.Application.OrdensServicos.DTOs.Response;

namespace AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces
{
    public interface IObterOrdemServicoPublicaPorIdUseCase
    {
        Task<OrdemServicoPublicoDetalheDto?> ExecuteAsync(Guid id);
    }
}
