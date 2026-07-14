using AutoReparos.Application.Servicos.UseCases.Interfaces;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Servicos.UseCases
{
    public class ExcluirServicoUseCase(IServicoRepository repository) : IExcluirServicoUseCase
    {
        public async Task ExecuteAsync(Guid id)
        {
            var servico = await repository.GetById(id)
                ?? throw new NotFoundException("Serviço não encontrado.");

            await repository.Delete(servico);
        }
    }
}
