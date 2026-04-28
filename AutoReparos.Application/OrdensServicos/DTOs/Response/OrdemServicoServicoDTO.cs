namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record OrdemServicoServicoDTO(
        Guid Id,
        Guid ServicoId,
        decimal ValorCobrado,
        string Status,
        DateTime? IniciadoEm,
        DateTime? ConcluidoEm,
        TimeSpan? TempoExecucao
    );
}
