using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Veiculos.DTOs.Request
{
    public record VeiculoPagedRequest : PagedRequest
    {
        public Guid? ClienteId { get; init; }
    }
}
