using System;
using System.Collections.Generic;
using AutoReparos.Domain.OrdensServicos.Enums;

namespace AutoReparos.Application.Dashboard.DTOs
{
    public record DashboardMetricsDto(
        decimal FaturamentoMesAtual,
        decimal FaturamentoMesAnterior,
        int OrdensEmExecucao,
        int TotalOrdensMesAtual,
        IEnumerable<DashboardOrdemServicoDto> UltimasOrdens,
        IEnumerable<DashboardInsumoCriticoDto> InsumosCriticos,
        IEnumerable<DashboardMensalStatusDto> HistoricoMensal,
        IEnumerable<DashboardVolumeDiarioDto>? VolumeDiario = null,
        DashboardTempoMedioStatusDto? TemposMedios = null);

    public record DashboardOrdemServicoDto(
        Guid Id,
        EStatusOrdemServico Status,
        decimal ValorTotal,
        DateTime CriadoEm,
        string ModeloVeiculo,
        string PlacaVeiculo);

    public record DashboardInsumoCriticoDto(Guid Id, string Nome, int QuantidadeEstoque);

    public record DashboardMensalStatusDto(string Mes, int TotalOrdens, decimal TotalFaturado, int TotalServicosRealizados);

    public record DashboardVolumeDiarioDto(
        DateOnly Data,
        int TotalCriadas,
        int TotalFinalizadas);

    public record DashboardTempoMedioStatusDto(
        double TempoMedioDiagnosticoHoras,
        double TempoMedioExecucaoHoras,
        double TempoMedioFinalizacaoHoras,
        string TempoMedioDiagnosticoFormatado,
        string TempoMedioExecucaoFormatado,
        string TempoMedioFinalizacaoFormatado,
        int TotalOrdensComDiagnostico,
        int TotalOrdensComExecucao,
        int TotalOrdensComFinalizacao);
}
