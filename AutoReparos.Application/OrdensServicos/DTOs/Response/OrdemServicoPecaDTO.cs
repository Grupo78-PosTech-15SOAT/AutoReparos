namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record OrdemServicoPecaDTO(
        Guid Id,
        Guid? PecaId,
        string Descricao,
        decimal ValorUnitario,
        int Quantidade,
        decimal ValorTotal,
        string Origem
    );
}
