using System.Collections.Generic;
using System.Threading.Tasks;
using AutoReparos.Application.OrdensServicos.DTOs.Response;

namespace AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces
{
    public interface IListarKanbanOrdensServicoUseCase
    {
        Task<IEnumerable<KanbanColumnDto>> ExecuteAsync();
    }
}
