using AutoReparos.Application.Usuarios.DTOs.Request;

namespace AutoReparos.Application.Usuarios.UseCases.Interfaces
{
    public interface IAtualizarUsuarioUseCase
    {
        Task ExecuteAsync(Guid id, UsuarioUpdateDto dto);
    }
}
