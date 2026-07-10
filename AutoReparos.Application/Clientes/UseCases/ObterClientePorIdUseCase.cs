using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Clientes.Mappers;
using AutoReparos.Application.Clientes.UseCases.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Clientes.UseCases
{
    public class ObterClientePorIdUseCase(IClienteRepository repository) : IObterClientePorIdUseCase
    {
        public async Task<ClienteDto?> ExecuteAsync(Guid id)
        {
            var cliente = await repository.GetById(id);
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            return ClienteMapper.ToDto(cliente);
        }
    }
}
