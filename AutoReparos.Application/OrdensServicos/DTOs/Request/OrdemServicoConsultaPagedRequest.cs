using AutoReparos.Application.Shared;

namespace AutoReparos.Application.OrdensServicos.DTOs.Request
{
    public record OrdemServicoConsultaPagedRequest : PagedRequest
    {
        public string? Documento { get; init; }
        public string? Placa { get; init; }
    }
}
