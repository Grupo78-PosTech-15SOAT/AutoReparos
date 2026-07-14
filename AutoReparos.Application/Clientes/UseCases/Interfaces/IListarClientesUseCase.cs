using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Clientes.UseCases.Interfaces
{
    public interface IListarClientesUseCase
    {
        Task<PagedResult<ClienteDto>> ExecuteAsync(ClientePagedRequest request);
    }
}
