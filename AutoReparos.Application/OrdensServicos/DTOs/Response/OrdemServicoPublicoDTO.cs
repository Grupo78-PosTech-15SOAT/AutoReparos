namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record OrdemServicoPublicoDto(
        Guid Id,
        string Status,
        string? Observacao,
        DateTime CriadoEm,
        DateTime? IniciadoEm,
        DateTime? FinalizadoEm,
        DateTime? EntregueEm
    );
}
