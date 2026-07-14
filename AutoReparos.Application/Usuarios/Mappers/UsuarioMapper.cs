using AutoReparos.Application.Usuarios.DTOs.Response;
using AutoReparos.Domain.Usuarios.Entities;

namespace AutoReparos.Application.Usuarios.Mappers
{
    public static class UsuarioMapper
    {
        public static UsuarioDto ToDto(Usuario usuario) => new(
            usuario.Id,
            usuario.NomeCompleto,
            usuario.Email.Endereco,
            usuario.Tipo,
            usuario.CriadoEm
        );
    }
}
