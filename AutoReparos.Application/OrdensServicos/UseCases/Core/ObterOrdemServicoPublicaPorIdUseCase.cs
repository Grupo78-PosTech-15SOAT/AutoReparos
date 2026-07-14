using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Mappers;
using AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces;
using AutoReparos.Domain.OrdensServicos.Repositories;

namespace AutoReparos.Application.OrdensServicos.UseCases.Core
{
    public class ObterOrdemServicoPublicaPorIdUseCase(IOrdemServicoRepository repository) : IObterOrdemServicoPublicaPorIdUseCase
    {
        public async Task<OrdemServicoPublicoDetalheDto?> ExecuteAsync(Guid id)
        {
            var os = await repository.GetById(id);
            return os is null ? null : OrdemServicoMapper.ToPublicDetalheDto(os);
        }
    }
}
