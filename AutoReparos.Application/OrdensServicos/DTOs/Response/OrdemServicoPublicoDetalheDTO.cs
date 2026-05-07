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
        IEnumerable<OrdemServicoServicoPublicoDTO> Servicos,
        IEnumerable<OrdemServicoInsumoPublicoDTO> Insumos
    );

    public record OrdemServicoServicoPublicoDTO(
        Guid Id,
        string Status,
        DateTime? IniciadoEm,
        DateTime? ConcluidoEm
    );

    public record OrdemServicoInsumoPublicoDTO(
        Guid Id,
        string Descricao,
        int Quantidade
    );
}
