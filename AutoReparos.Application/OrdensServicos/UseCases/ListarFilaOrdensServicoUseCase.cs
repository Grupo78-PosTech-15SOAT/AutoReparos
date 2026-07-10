using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Mappers;
using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.OrdensServicos.Repositories;

namespace AutoReparos.Application.OrdensServicos.UseCases
{
    public class ListarFilaOrdensServicoUseCase(IOrdemServicoRepository repository) : IListarFilaOrdensServicoUseCase
    {
        public async Task<PagedResult<OrdemServicoDto>> ExecuteAsync(PagedRequest request)
        {
            var (items, total) = await repository.GetFila(request.Skip, request.PageSize);
            return new PagedResult<OrdemServicoDto>(items.Select(OrdemServicoMapper.ToDto), total, request.PageNumber, request.PageSize);
        }
    }
}
