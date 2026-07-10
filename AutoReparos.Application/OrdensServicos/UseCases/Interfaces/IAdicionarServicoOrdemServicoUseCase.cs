using AutoReparos.Application.OrdensServicos.DTOs.Request;

namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces
{
    public interface IAdicionarServicoOrdemServicoUseCase
    {
        Task ExecuteAsync(Guid id, AdicionarServicoDto dto);
    }
}
