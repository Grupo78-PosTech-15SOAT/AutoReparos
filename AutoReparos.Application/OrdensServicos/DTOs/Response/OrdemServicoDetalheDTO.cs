namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record OrdemServicoDetalheDto(
        Guid Id,
        Guid ClienteId,
        string? ClienteNome,
        Guid VeiculoId,
        string? PlacaVeiculo,
        string? ModeloVeiculo,
        string Status,
        string? Observacao,
        decimal ValorTotal,
        DateTime CriadoEm,
        DateTime? IniciadoEm,
        DateTime? FinalizadoEm,
        DateTime? EntregueEm,
        DateTime? EnvioAprovacaoEm,
        string? ResponsavelId,
        string? ResponsavelNome,
        IEnumerable<OrdemServicoServicoDto> Servicos,
        IEnumerable<OrdemServicoInsumoDto> Insumos
    );
}
