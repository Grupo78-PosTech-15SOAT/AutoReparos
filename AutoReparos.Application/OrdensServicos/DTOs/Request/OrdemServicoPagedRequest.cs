using AutoReparos.Application.Shared;
using AutoReparos.Domain.OrdensServicos.Enums;

namespace AutoReparos.Application.OrdensServicos.DTOs.Request
{
    public record OrdemServicoPagedRequest : PagedRequest
    {
        public Guid? ClienteId { get; init; }
        public Guid? VeiculoId { get; init; }
        public EStatusOrdemServico? Status { get; init; }
    }
}
