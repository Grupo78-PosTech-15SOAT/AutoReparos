namespace AutoReparos.Application.Insumos.DTOs.Response
{
    public record InsumoDto(
        Guid Id,
        string Nome,
        string? Descricao,
        decimal Valor,
        int QuantidadeEstoque,
        DateTime CriadoEm,
        DateTime? AtualizadoEm
    );
}
