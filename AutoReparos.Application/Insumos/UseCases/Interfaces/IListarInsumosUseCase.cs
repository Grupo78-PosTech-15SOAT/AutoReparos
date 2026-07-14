using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Insumos.UseCases.Interfaces
{
    public interface IListarInsumosUseCase
    {
        Task<PagedResult<InsumoDto>> ExecuteAsync(InsumoPagedRequest request);
    }
}
