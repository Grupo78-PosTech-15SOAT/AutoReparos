namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record OrdemServicoPublicoDetalheDto(
        Guid Id,
        string Status,
        string? Observacao,
        DateTime CriadoEm,
        DateTime? IniciadoEm,
        DateTime? FinalizadoEm,
        DateTime? EntregueEm,
        IEnumerable<OrdemServicoServicoPublicoDto> Servicos,
        IEnumerable<OrdemServicoInsumoPublicoDto> Insumos
    );

    public record OrdemServicoServicoPublicoDto(
        Guid Id,
        string Status,
        DateTime? IniciadoEm,
        DateTime? ConcluidoEm
    );

    public record OrdemServicoInsumoPublicoDto(
        Guid Id,
        string Descricao,
        int Quantidade
    );
}
