using AutoReparos.Application.Insumos.DTOs.Response;

namespace AutoReparos.Application.Insumos.UseCases.Interfaces
{
    public interface IObterInsumoPorIdUseCase
    {
        Task<InsumoDto?> ExecuteAsync(Guid id);
    }
}
