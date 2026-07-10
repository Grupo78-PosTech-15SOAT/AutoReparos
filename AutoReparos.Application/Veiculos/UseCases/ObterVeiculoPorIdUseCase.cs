using AutoReparos.Application.Veiculos.DTOs.Response;
using AutoReparos.Application.Veiculos.Mappers;
using AutoReparos.Application.Veiculos.UseCases.Interfaces;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Repositories;

namespace AutoReparos.Application.Veiculos.UseCases
{
    public class ObterVeiculoPorIdUseCase(IVeiculoRepository repository) : IObterVeiculoPorIdUseCase
    {
        public async Task<VeiculoDto?> ExecuteAsync(Guid id)
        {
            var veiculo = await repository.GetById(id);

            if (veiculo == null)
                throw new NotFoundException("Veículo não encontrado.");

            return VeiculoMapper.ToDto(veiculo);
        }
    }
}
