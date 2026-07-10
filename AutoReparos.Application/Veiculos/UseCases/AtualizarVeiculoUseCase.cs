using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.UseCases.Interfaces;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Repositories;

namespace AutoReparos.Application.Veiculos.UseCases
{
    public class AtualizarVeiculoUseCase(IVeiculoRepository repository) : IAtualizarVeiculoUseCase
    {
        public async Task ExecuteAsync(Guid id, VeiculoUpdateDto dto)
        {
            var veiculo = await repository.GetById(id);

            if (veiculo == null)
                throw new NotFoundException("Veículo não encontrado.");

            veiculo.Atualizar(
                dto.Marca,
                dto.Modelo,
                dto.AnoFabricacao,
                dto.AnoModelo
            );

            await repository.Update(veiculo);
        }
    }
}
