using AutoReparos.Application.Usuarios.DTOs.Response;

namespace AutoReparos.Application.Usuarios.UseCases.Interfaces
{
    public interface IObterUsuarioPorIdUseCase
    {
        Task<UsuarioDto?> ExecuteAsync(Guid id);
    }
}
