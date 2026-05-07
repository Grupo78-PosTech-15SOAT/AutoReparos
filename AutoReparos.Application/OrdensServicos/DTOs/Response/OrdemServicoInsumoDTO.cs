namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record OrdemServicoInsumoDto(
        Guid Id,
        Guid? InsumoId,
        string Descricao,
        decimal ValorUnitario,
        int Quantidade,
        decimal ValorTotal,
        string Origem
    );
}
