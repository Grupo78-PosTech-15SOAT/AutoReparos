using AutoReparos.Domain.Usuarios.Enums;

namespace AutoReparos.Application.Usuarios.DTOs.Request
{
    public record UsuarioUpdateDTO(
        string NomeCompleto,
        ETipoUsuario Tipo
    );
}
