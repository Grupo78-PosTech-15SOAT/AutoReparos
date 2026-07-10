using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Mappers;
using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.OrdensServicos.Repositories;

namespace AutoReparos.Application.OrdensServicos.UseCases
{
    public class ListarOrdensServicoUseCase(IOrdemServicoRepository repository) : IListarOrdensServicoUseCase
    {
        public async Task<PagedResult<OrdemServicoDto>> ExecuteAsync(OrdemServicoPagedRequest request)
        {
            var (items, total) = await repository.GetAll(request.ClienteId, request.VeiculoId, request.Status, request.Skip, request.PageSize);
            return new PagedResult<OrdemServicoDto>(items.Select(OrdemServicoMapper.ToDto), total, request.PageNumber, request.PageSize);
        }
    }
}
