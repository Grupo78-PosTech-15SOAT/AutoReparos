using AutoReparos.Domain.Usuarios.Enums;

namespace AutoReparos.Application.Usuarios.DTOs.Response
{
    public record UsuarioDto(
        Guid Id,
        string NomeCompleto,
        string Email,
        ETipoUsuario Tipo,
        DateTime CriadoEm
    );
}
