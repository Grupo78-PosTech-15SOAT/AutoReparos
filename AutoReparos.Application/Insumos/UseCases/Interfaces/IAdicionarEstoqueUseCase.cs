using AutoReparos.Application.Insumos.DTOs.Request;

namespace AutoReparos.Application.Insumos.UseCases.Interfaces
{
    public interface IAdicionarEstoqueUseCase
    {
        Task ExecuteAsync(Guid id, AtualizarEstoqueDto dto);
    }
}
