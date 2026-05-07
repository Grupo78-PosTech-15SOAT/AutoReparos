using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Clientes.DTOs.Request
{
    public record ClientePagedRequest : PagedRequest
    {
        public string? Nome { get; init; }
    }
}
