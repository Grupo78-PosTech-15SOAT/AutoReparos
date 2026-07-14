using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.UseCases.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Clientes.UseCases
{
    public class AtualizarClienteUseCase(IClienteRepository repository) : IAtualizarClienteUseCase
    {
        public async Task ExecuteAsync(Guid id, ClienteUpdateDto dto)
        {
            var cliente = await repository.GetById(id);
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            cliente.Atualizar(dto.Nome, dto.Email, dto.Telefone);
            await repository.Update(cliente);
        }
    }
}
