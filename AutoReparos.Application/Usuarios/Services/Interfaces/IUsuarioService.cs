using AutoReparos.Application.Shared;
using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.DTOs.Response;

namespace AutoReparos.Application.Usuarios.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioDTO> Create(UsuarioCreateDTO dto);
        Task<UsuarioDTO?> GetById(Guid id);
        Task<PagedResult<UsuarioDTO>> GetAll(PagedRequest request);
        Task Update(Guid id, UsuarioUpdateDTO dto);
        Task Delete(Guid id);
    }
}
