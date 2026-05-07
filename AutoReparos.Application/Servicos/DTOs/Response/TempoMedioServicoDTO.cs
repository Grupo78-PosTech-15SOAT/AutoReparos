namespace AutoReparos.Application.Servicos.DTOs.Response
{
    public record TempoMedioServicoDto(
        Guid ServicoId,
        string NomeServico,
        TimeSpan TempoMedio,
        int TotalExecucoes
    );
}
