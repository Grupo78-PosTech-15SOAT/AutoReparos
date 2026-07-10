using AutoReparos.Application.Veiculos.DTOs.Request;

namespace AutoReparos.Application.Veiculos.UseCases.Interfaces
{
    public interface IAtualizarVeiculoUseCase
    {
        Task ExecuteAsync(Guid id, VeiculoUpdateDto dto);
    }
}
