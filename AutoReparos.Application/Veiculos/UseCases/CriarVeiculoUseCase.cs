using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;
using AutoReparos.Application.Veiculos.Mappers;
using AutoReparos.Application.Veiculos.UseCases.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;

namespace AutoReparos.Application.Veiculos.UseCases
{
    public class CriarVeiculoUseCase(IVeiculoRepository repository, IClienteRepository clienteRepository) : ICriarVeiculoUseCase
    {
        public async Task<VeiculoDto> ExecuteAsync(VeiculoCreateDto dto)
        {
            var cliente = await clienteRepository.GetById(dto.ClienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            var veiculo = new Veiculo(
                dto.ClienteId,
                dto.Marca,
                dto.Modelo,
                dto.AnoFabricacao,
                dto.AnoModelo,
                new Placa(dto.Placa),
                new Chassi(dto.Chassi),
                new Renavam(dto.Renavam)
            );

            await repository.Create(veiculo);

            return VeiculoMapper.ToDto(veiculo);
        }
    }
}
