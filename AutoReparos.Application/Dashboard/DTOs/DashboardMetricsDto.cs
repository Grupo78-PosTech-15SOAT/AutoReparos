using AutoReparos.Domain.OrdensServicos.Enums;

namespace AutoReparos.Application.Dashboard.DTOs
{
    public record DashboardMetricsDto(
        decimal FaturamentoTotal,
        int OrdensEmExecucao,
        int TotalOrdensServico,
        IEnumerable<DashboardOrdemServicoDto> UltimasOrdens,
        IEnumerable<DashboardInsumoCriticoDto> InsumosCriticos);

    public record DashboardOrdemServicoDto(
        Guid Id,
        EStatusOrdemServico Status,
        decimal ValorTotal,
        DateTime CriadoEm,
        string ModeloVeiculo,
        string PlacaVeiculo);

    public record DashboardInsumoCriticoDto(Guid Id, string Nome, int QuantidadeEstoque);
}
