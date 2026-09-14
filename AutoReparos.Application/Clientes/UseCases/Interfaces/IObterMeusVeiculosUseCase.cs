using AutoReparos.Application.Veiculos.DTOs.Response;

namespace AutoReparos.Application.Clientes.UseCases.Interfaces;

public interface IObterMeusVeiculosUseCase
{
    Task<IEnumerable<VeiculoDto>> ExecuteAsync(Guid clienteId, CancellationToken cancellationToken = default);
}
