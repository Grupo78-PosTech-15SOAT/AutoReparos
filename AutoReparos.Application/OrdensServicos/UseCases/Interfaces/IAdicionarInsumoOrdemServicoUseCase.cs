using AutoReparos.Application.OrdensServicos.DTOs.Request;

namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces
{
    public interface IAdicionarInsumoOrdemServicoUseCase
    {
        Task ExecuteAsync(Guid id, AdicionarInsumoDto dto);
    }
}
