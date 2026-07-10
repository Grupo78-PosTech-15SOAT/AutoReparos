using AutoReparos.Application.OrdensServicos.DTOs.Request;

namespace AutoReparos.Application.OrdensServicos.UseCases.Fluxo.Interfaces
{
    public interface IAdicionarInsumoOrdemServicoUseCase
    {
        Task ExecuteAsync(Guid id, AdicionarInsumoDto dto);
    }
}
