using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Mappers;
using AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces;
using AutoReparos.Domain.OrdensServicos.Repositories;

namespace AutoReparos.Application.OrdensServicos.UseCases.Core
{
    public class ObterOrdemServicoPorIdUseCase(IOrdemServicoRepository repository) : IObterOrdemServicoPorIdUseCase
    {
        public async Task<OrdemServicoDetalheDto?> ExecuteAsync(Guid id)
        {
            var os = await repository.GetById(id);
            return os is null ? null : OrdemServicoMapper.ToDetalheDto(os);
        }
    }
}
