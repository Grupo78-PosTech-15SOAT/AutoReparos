using AutoReparos.Application.Veiculos.DTOs.Response;

namespace AutoReparos.Application.Veiculos.UseCases.Interfaces
{
    public interface IObterVeiculoPorPlacaUseCase
    {
        Task<VeiculoDto?> ExecuteAsync(string placa);
    }
}
