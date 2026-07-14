using AutoReparos.Application.Insumos.DTOs.Request;

namespace AutoReparos.Application.Insumos.UseCases.Interfaces
{
    public interface IRemoverEstoqueUseCase
    {
        Task ExecuteAsync(Guid id, AtualizarEstoqueDto dto);
    }
}
