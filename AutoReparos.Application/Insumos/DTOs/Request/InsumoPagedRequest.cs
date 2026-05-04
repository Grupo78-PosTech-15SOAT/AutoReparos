using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Insumos.DTOs.Request
{
    public record InsumoPagedRequest : PagedRequest
    {
        public string? Nome { get; init; }
    }
}
