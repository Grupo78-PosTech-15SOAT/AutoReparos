using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.OrdensServicos.UseCases
{
    public class AdicionarServicoOrdemServicoUseCase(IOrdemServicoRepository repository) : IAdicionarServicoOrdemServicoUseCase
    {
        public async Task ExecuteAsync(Guid id, AdicionarServicoDto dto)
        {
            var os = await repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            var item = new OrdemServicoServico(id, dto.ServicoId, dto.ValorCobrado);
            os.AdicionarServico(item);
            await repository.Update(os);
        }
    }
}
