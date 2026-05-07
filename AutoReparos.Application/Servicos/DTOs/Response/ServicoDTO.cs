namespace AutoReparos.Application.Servicos.DTOs.Response
{
    public record ServicoDto(
        Guid Id,
        string Nome,
        string? Descricao,
        decimal? ValorTabelado,
        DateTime CriadoEm,
        DateTime? AtualizadoEm
    );
}
