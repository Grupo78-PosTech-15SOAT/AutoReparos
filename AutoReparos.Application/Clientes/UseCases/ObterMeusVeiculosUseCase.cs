using AutoReparos.Application.Clientes.UseCases.Interfaces;
using AutoReparos.Application.Veiculos.DTOs.Response;
using AutoReparos.Application.Veiculos.Mappers;
using AutoReparos.Domain.Veiculos.Repositories;

namespace AutoReparos.Application.Clientes.UseCases;

public class ObterMeusVeiculosUseCase(IVeiculoRepository veiculoRepository) : IObterMeusVeiculosUseCase
{
    public async Task<IEnumerable<VeiculoDto>> ExecuteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        if (clienteId == Guid.Empty)
            return Enumerable.Empty<VeiculoDto>();

        // Busca todos os veículos pertencentes exclusivamente a este cliente
        var (items, _) = await veiculoRepository.GetAll(clienteId, skip: 0, take: 100);

        return items.Select(VeiculoMapper.ToDto);
    }
}
