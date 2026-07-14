using AutoReparos.Application.Veiculos.DTOs.Response;
using AutoReparos.Application.Veiculos.Mappers;
using AutoReparos.Application.Veiculos.UseCases.Interfaces;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Repositories;

namespace AutoReparos.Application.Veiculos.UseCases
{
    public class ObterVeiculoPorPlacaUseCase(IVeiculoRepository repository) : IObterVeiculoPorPlacaUseCase
    {
        public async Task<VeiculoDto?> ExecuteAsync(string placa)
        {
            var veiculo = await repository.GetByPlaca(placa);

            if (veiculo == null)
                throw new NotFoundException("Veículo não encontrado.");

            return VeiculoMapper.ToDto(veiculo);
        }
    }
}
