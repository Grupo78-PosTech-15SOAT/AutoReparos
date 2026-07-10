using AutoReparos.Application.Clientes.UseCases.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Clientes.UseCases
{
    public class ExcluirClienteUseCase(IClienteRepository repository) : IExcluirClienteUseCase
    {
        public async Task ExecuteAsync(Guid id)
        {
            var cliente = await repository.GetById(id);
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            await repository.Delete(cliente);
        }
    }
}
