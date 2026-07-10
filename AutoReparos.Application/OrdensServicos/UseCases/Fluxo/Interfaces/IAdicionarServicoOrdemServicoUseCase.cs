using AutoReparos.Application.OrdensServicos.DTOs.Request;

namespace AutoReparos.Application.OrdensServicos.UseCases.Fluxo.Interfaces
{
    public interface IAdicionarServicoOrdemServicoUseCase
    {
        Task ExecuteAsync(Guid id, AdicionarServicoDto dto);
    }
}
