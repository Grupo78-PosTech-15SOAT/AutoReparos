namespace AutoReparos.Application.Servicos.DTOs.Response
{
    public record TempoMedioServicoDTO(
        Guid ServicoId,
        string NomeServico,
        TimeSpan TempoMedio,
        int TotalExecucoes
    );
}
