using AutoReparos.Domain.Usuarios.Enums;

namespace AutoReparos.Application.Usuarios.DTOs.Response
{
    public record UsuarioDTO(
        Guid Id,
        string NomeCompleto,
        string Email,
        ETipoUsuario Tipo,
        DateTime CriadoEm
    );
}
