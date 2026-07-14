using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.DTOs.Response;
using AutoReparos.Application.Insumos.Mappers;
using AutoReparos.Application.Insumos.UseCases.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.Insumos.Repositories;

namespace AutoReparos.Application.Insumos.UseCases
{
    public class ListarInsumosUseCase(IInsumoRepository repository) : IListarInsumosUseCase
    {
        public async Task<PagedResult<InsumoDto>> ExecuteAsync(InsumoPagedRequest request)
        {
            var (items, total) = await repository.GetAll(request.Nome, request.Skip, request.PageSize);
            return new PagedResult<InsumoDto>(items.Select(InsumoMapper.ToDto), total, request.PageNumber, request.PageSize);
        }
    }
}
