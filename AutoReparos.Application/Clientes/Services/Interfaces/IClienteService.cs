using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Clientes.Services.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteDTO> Create(ClienteCreateDTO dto);
        Task<ClienteDTO?> GetById(Guid id);
        Task<PagedResult<ClienteDTO>> GetAll(PagedRequest request);
        Task Update(Guid id, ClienteUpdateDTO dto);
        Task Delete(Guid id);
    }
}
