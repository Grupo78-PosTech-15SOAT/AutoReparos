using AutoReparos.Application.OrdensServicos.DTOs.Response;

namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces;

public interface IObterMinhasOrdensServicoUseCase
{
    Task<IEnumerable<MinhaOrdemServicoDto>> ExecuteAsync(Guid clienteId, string? placa = null, CancellationToken cancellationToken = default);
}
