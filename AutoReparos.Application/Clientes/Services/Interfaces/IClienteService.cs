using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Clientes.Services.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteDto> Create(ClienteCreateDto dto);
        Task<ClienteDto?> GetById(Guid id);
        Task<PagedResult<ClienteDto>> GetAll(ClientePagedRequest request);
        Task Update(Guid id, ClienteUpdateDto dto);
        Task Delete(Guid id);
    }
}
