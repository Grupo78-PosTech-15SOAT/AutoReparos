using AutoReparos.Application.Shared;
using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.DTOs.Response;

namespace AutoReparos.Application.Usuarios.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioDto> Create(UsuarioCreateDto dto);
        Task<UsuarioDto?> GetById(Guid id);
        Task<PagedResult<UsuarioDto>> GetAll(UsuarioPagedRequest request);
        Task Update(Guid id, UsuarioUpdateDto dto);
        Task Delete(Guid id);
    }
}
