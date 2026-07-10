using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.OrdensServicos.UseCases
{
    public class IniciarServicoOrdemServicoUseCase(IOrdemServicoRepository repository) : IIniciarServicoOrdemServicoUseCase
    {
        public async Task ExecuteAsync(Guid ordemServicoId, Guid ordemServicoServicoId)
        {
            var os = await repository.GetById(ordemServicoId)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            os.IniciarServico(ordemServicoServicoId);
            await repository.Update(os);
        }
    }
}
