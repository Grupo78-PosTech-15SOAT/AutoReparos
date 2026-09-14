namespace AutoReparos.Application.OrdensServicos.DTOs.Response;

public record MinhaOrdemServicoDto(
    Guid Id,
    Guid VeiculoId,
    string? ModeloVeiculo,
    string? PlacaVeiculo,
    string Status,
    string? Observacao,
    decimal ValorTotal,
    DateTime CriadoEm,
    DateTime? IniciadoEm,
    DateTime? FinalizadoEm,
    DateTime? EntregueEm
);
