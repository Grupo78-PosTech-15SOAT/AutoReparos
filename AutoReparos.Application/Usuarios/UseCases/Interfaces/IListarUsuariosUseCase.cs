using AutoReparos.Application.Shared;
using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.DTOs.Response;

namespace AutoReparos.Application.Usuarios.UseCases.Interfaces
{
    public interface IListarUsuariosUseCase
    {
        Task<PagedResult<UsuarioDto>> ExecuteAsync(UsuarioPagedRequest request);
    }
}
