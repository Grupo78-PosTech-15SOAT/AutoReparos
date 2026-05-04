using AutoReparos.Application.Shared;
using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;
using AutoReparos.Application.Veiculos.Services.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;

namespace AutoReparos.Application.Veiculos.Services
{
    public class VeiculoService : IVeiculoService
    {
        private readonly IVeiculoRepository _repository;
        private readonly IClienteRepository _clienteRepository;

        public VeiculoService(
            IVeiculoRepository repository,
            IClienteRepository clienteRepository)
        {
            _repository = repository;
            _clienteRepository = clienteRepository;
        }

        public async Task<VeiculoDTO> Create(VeiculoCreateDTO dto)
        {
            var cliente = await _clienteRepository.GetById(dto.ClienteId);
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

            await _repository.Create(veiculo);

            return ToDTO(veiculo);
        }

        public async Task<PagedResult<VeiculoDTO>> GetAll(VeiculoPagedRequest request)
        {
            var (items, total) = await _repository.GetAll(request.ClienteId, request.Skip, request.PageSize);
            return new PagedResult<VeiculoDTO>(items.Select(ToDTO), total, request.PageNumber, request.PageSize);
        }

        public async Task<VeiculoDTO?> GetById(Guid id)
        {
            var veiculo = await _repository.GetById(id);

            if (veiculo == null)
                throw new NotFoundException("Veículo não encontrado.");

            return ToDTO(veiculo);
        }

        public async Task<VeiculoDTO?> GetByPlaca(string placa)
        {
            var veiculo = await _repository.GetByPlaca(placa);

            if (veiculo == null)
                throw new NotFoundException("Veículo não encontrado.");

            return ToDTO(veiculo);
        }

        public async Task Update(Guid id, VeiculoUpdateDTO dto)
        {
            var veiculo = await _repository.GetById(id);

            if (veiculo == null)
                throw new NotFoundException("Veículo não encontrado.");

            veiculo.Atualizar(
                dto.Marca,
                dto.Modelo,
                dto.AnoFabricacao,
                dto.AnoModelo
            );

            await _repository.Update(veiculo);
        }

        public async Task Delete(Guid id)
        {
            var veiculo = await _repository.GetById(id);

            if (veiculo == null)
                throw new NotFoundException("Veículo não encontrado.");

            await _repository.Delete(veiculo);
        }

        private static VeiculoDTO ToDTO(Veiculo v) => new(
            v.Id,
            v.ClienteId,
            v.Marca,
            v.Modelo,
            v.AnoFabricacao,
            v.AnoModelo,
            v.Placa.Valor,
            v.Chassi.Valor,
            v.Renavam.Valor
        );
    }
}
