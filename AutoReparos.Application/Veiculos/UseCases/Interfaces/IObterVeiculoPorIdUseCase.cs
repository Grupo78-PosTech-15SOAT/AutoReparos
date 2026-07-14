using AutoReparos.Application.Veiculos.DTOs.Response;

namespace AutoReparos.Application.Veiculos.UseCases.Interfaces
{
    public interface IObterVeiculoPorIdUseCase
    {
        Task<VeiculoDto?> ExecuteAsync(Guid id);
    }
}
