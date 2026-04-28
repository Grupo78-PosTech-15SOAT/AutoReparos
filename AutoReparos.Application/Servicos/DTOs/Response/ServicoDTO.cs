namespace AutoReparos.Application.Servicos.DTOs.Response
{
    public record ServicoDTO(
        Guid Id,
        string Nome,
        string? Descricao,
        decimal? ValorTabelado,
        DateTime CriadoEm,
        DateTime? AtualizadoEm
    );
}
