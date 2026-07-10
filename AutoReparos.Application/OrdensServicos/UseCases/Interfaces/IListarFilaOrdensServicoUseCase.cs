using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces
{
    public interface IListarFilaOrdensServicoUseCase
    {
        Task<PagedResult<OrdemServicoDto>> ExecuteAsync(PagedRequest request);
    }
}
