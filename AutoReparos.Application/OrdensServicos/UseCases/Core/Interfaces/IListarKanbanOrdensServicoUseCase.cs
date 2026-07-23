using System.Threading.Tasks;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces
{
    public interface IListarKanbanOrdensServicoUseCase
    {
        Task<PagedResult<OrdemServicoKanbanDto>> ExecuteAsync(PagedRequest request);
    }
}
