using AutoReparos.Domain.Usuarios.Enums;

namespace AutoReparos.Application.Usuarios.DTOs.Request
{
    public record UsuarioCreateDTO(
        string NomeCompleto,
        string Email,
        string Password,
        ETipoUsuario Tipo
    );
}
