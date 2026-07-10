using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Servicos.UseCases.Interfaces
{
    public interface IListarServicosUseCase
    {
        Task<PagedResult<ServicoDto>> ExecuteAsync(ServicoPagedRequest request);
    }
}
