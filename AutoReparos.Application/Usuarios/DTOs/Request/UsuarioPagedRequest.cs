using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Usuarios.DTOs.Request
{
    public record UsuarioPagedRequest : PagedRequest
    {
        public string? Nome { get; init; }
    }
}
