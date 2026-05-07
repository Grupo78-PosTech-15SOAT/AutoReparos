using AutoReparos.Domain.Usuarios.Enums;

namespace AutoReparos.Application.Usuarios.DTOs.Request
{
    public record UsuarioCreateDto(
        string NomeCompleto,
        string Email,
        string Password,
        ETipoUsuario Tipo
    );
}
