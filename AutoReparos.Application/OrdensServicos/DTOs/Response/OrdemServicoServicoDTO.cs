namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record OrdemServicoServicoDto(
        Guid Id,
        Guid ServicoId,
        decimal ValorCobrado,
        string Status,
        DateTime? IniciadoEm,
        DateTime? ConcluidoEm,
        TimeSpan? TempoExecucao
    );
}
