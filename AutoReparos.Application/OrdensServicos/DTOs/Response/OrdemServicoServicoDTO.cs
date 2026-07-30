namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record OrdemServicoServicoDto(
        Guid Id,
        Guid ServicoId,
        string? NomeServico,
        decimal ValorCobrado,
        string Status,
        DateTime? IniciadoEm,
        DateTime? ConcluidoEm,
        TimeSpan? TempoExecucao
    );
}
