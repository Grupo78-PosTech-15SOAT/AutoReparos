namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record OrdemServicoDetalheDto(
        Guid Id,
        Guid ClienteId,
        Guid VeiculoId,
        string Status,
        string? Observacao,
        decimal ValorTotal,
        DateTime CriadoEm,
        DateTime? IniciadoEm,
        DateTime? FinalizadoEm,
        DateTime? EntregueEm,
        IEnumerable<OrdemServicoServicoDto> Servicos,
        IEnumerable<OrdemServicoInsumoDto> Insumos
    );
}
