using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Servicos.DTOs.Request
{
    public record ServicoPagedRequest : PagedRequest
    {
        public string? Nome { get; init; }
    }
}
