using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.DTOs.Response;

namespace AutoReparos.Application.Usuarios.UseCases.Interfaces
{
    public interface ICriarUsuarioUseCase
    {
        Task<UsuarioDto> ExecuteAsync(UsuarioCreateDto dto);
    }
}
