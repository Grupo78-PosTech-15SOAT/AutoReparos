using AutoReparos.Application.Insumos.DTOs.Request;

namespace AutoReparos.Application.Insumos.UseCases.Interfaces
{
    public interface IAtualizarInsumoUseCase
    {
        Task ExecuteAsync(Guid id, AtualizarInsumoDto dto);
    }
}
