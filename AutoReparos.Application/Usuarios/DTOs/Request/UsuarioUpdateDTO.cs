using AutoReparos.Domain.Usuarios.Enums;

namespace AutoReparos.Application.Usuarios.DTOs.Request
{
    public record UsuarioUpdateDto(
        string NomeCompleto,
        ETipoUsuario Tipo
    );
}
