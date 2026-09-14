using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoReparos.Application.Dashboard.DTOs;
using AutoReparos.Application.Dashboard.Services;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Infra.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AutoReparos.Infra.Services
{
    public class DashboardQueryService(AppDbContext context) : IDashboardQueryService
    {
        private const int EstoqueMinimo = 5;

        public async Task<DashboardMetricsDto> GetMetricsAsync()
        {
            var agora = DateTime.UtcNow;
            var inicioMesAtual = new DateTime(agora.Year, agora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var inicioMesAnterior = inicioMesAtual.AddMonths(-1);
            var inicioSeisMesesAtras = inicioMesAtual.AddMonths(-5);

            var ordensEmExecucao = await context.OrdensServico
                .AsNoTracking()
                .CountAsync(os => os.Status == EStatusOrdemServico.EmExecucao);

            var totalOrdensMesAtual = await context.OrdensServico
                .AsNoTracking()
                .CountAsync(os => os.CriadoEm >= inicioMesAtual);

            var faturamentoMesAtual = await context.OrdensServico
                .AsNoTracking()
                .Where(os => os.CriadoEm >= inicioMesAtual && (os.Status == EStatusOrdemServico.Finalizada || os.Status == EStatusOrdemServico.Entregue))
                .SumAsync(os => (os.Servicos.Sum(s => (decimal?)s.ValorCobrado) ?? 0m) + (os.Insumos.Sum(i => (decimal?)(i.ValorUnitario * i.Quantidade)) ?? 0m));

            var faturamentoMesAnterior = await context.OrdensServico
                .AsNoTracking()
                .Where(os => os.CriadoEm >= inicioMesAnterior && os.CriadoEm < inicioMesAtual && (os.Status == EStatusOrdemServico.Finalizada || os.Status == EStatusOrdemServico.Entregue))
                .SumAsync(os => (os.Servicos.Sum(s => (decimal?)s.ValorCobrado) ?? 0m) + (os.Insumos.Sum(i => (decimal?)(i.ValorUnitario * i.Quantidade)) ?? 0m));

            // Execute grouping entirely on SGBD
            var rawHistory = await context.OrdensServico
                .AsNoTracking()
                .Where(os => os.CriadoEm >= inicioSeisMesesAtras)
                .GroupBy(os => new { Year = os.CriadoEm.Year, Month = os.CriadoEm.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalOrdens = g.Count(),
                    TotalFaturado = g.Where(os => os.Status == EStatusOrdemServico.Finalizada || os.Status == EStatusOrdemServico.Entregue)
                                     .Sum(os => (os.Servicos.Sum(s => (decimal?)s.ValorCobrado) ?? 0m) + (os.Insumos.Sum(i => (decimal?)(i.ValorUnitario * i.Quantidade)) ?? 0m)),
                    TotalServicosRealizados = g.Where(os => os.Status == EStatusOrdemServico.Finalizada || os.Status == EStatusOrdemServico.Entregue)
                                              .Sum(os => os.Servicos.Count(s => s.Status == EStatusServicoOS.Concluido))
                })
                .ToListAsync();

            var historico = new List<DashboardMensalStatusDto>();
            for (int i = 5; i >= 0; i--)
            {
                var mesRef = inicioMesAtual.AddMonths(-i);
                var match = rawHistory.FirstOrDefault(h => h.Year == mesRef.Year && h.Month == mesRef.Month);

                var totalOrdens = match?.TotalOrdens ?? 0;
                var totalFaturado = match?.TotalFaturado ?? 0m;
                var totalServicosRealizados = match?.TotalServicosRealizados ?? 0;

                var nomeMes = mesRef.ToString("MMM/yyyy", new CultureInfo("pt-BR"));
                historico.Add(new DashboardMensalStatusDto(nomeMes, totalOrdens, totalFaturado, totalServicosRealizados));
            }

            var ultimasOrdens = await (
                from os in context.OrdensServico.AsNoTracking()
                join veiculo in context.Veiculos.AsNoTracking() on os.VeiculoId equals veiculo.Id
                orderby os.CriadoEm descending
                select new DashboardOrdemServicoDto(
                    os.Id,
                    os.Status,
                    (os.Servicos.Sum(s => (decimal?)s.ValorCobrado) ?? 0m) + (os.Insumos.Sum(i => (decimal?)(i.ValorUnitario * i.Quantidade)) ?? 0m),
                    os.CriadoEm,
                    veiculo.Modelo,
                    veiculo.Placa.Valor))
                .Take(5)
                .ToListAsync();

            var insumosCriticos = await context.Insumos
                .AsNoTracking()
                .Where(insumo => insumo.QuantidadeEstoque <= EstoqueMinimo)
                .OrderBy(insumo => insumo.QuantidadeEstoque)
                .ThenBy(insumo => insumo.Nome)
                .Select(insumo => new DashboardInsumoCriticoDto(insumo.Id, insumo.Nome, insumo.QuantidadeEstoque))
                .ToListAsync();

            var volumeDiario = await GetVolumeDiarioAsync(30);
            var temposMedios = await GetTemposMediosStatusAsync();

            return new DashboardMetricsDto(
                faturamentoMesAtual,
                faturamentoMesAnterior,
                ordensEmExecucao,
                totalOrdensMesAtual,
                ultimasOrdens,
                insumosCriticos,
                historico,
                volumeDiario,
                temposMedios);
        }

        public async Task<IEnumerable<DashboardVolumeDiarioDto>> GetVolumeDiarioAsync(int dias = 30)
        {
            if (dias <= 0) dias = 30;
            if (dias > 365) dias = 365;

            var hojeUtc = DateTime.UtcNow.Date;
            var dataInicioUtc = hojeUtc.AddDays(-(dias - 1));

            var ordens = await context.OrdensServico
                .AsNoTracking()
                .Where(os => os.CriadoEm >= dataInicioUtc || (os.FinalizadoEm != null && os.FinalizadoEm >= dataInicioUtc))
                .Select(os => new { os.CriadoEm, os.FinalizadoEm })
                .ToListAsync();

            var resultado = new List<DashboardVolumeDiarioDto>(dias);
            for (int i = 0; i < dias; i++)
            {
                var dia = DateOnly.FromDateTime(dataInicioUtc.AddDays(i));
                var criadas = ordens.Count(o => DateOnly.FromDateTime(o.CriadoEm) == dia);
                var finalizadas = ordens.Count(o => o.FinalizadoEm.HasValue && DateOnly.FromDateTime(o.FinalizadoEm.Value) == dia);
                resultado.Add(new DashboardVolumeDiarioDto(dia, criadas, finalizadas));
            }

            return resultado;
        }

        public async Task<DashboardTempoMedioStatusDto> GetTemposMediosStatusAsync()
        {
            var ordens = await context.OrdensServico
                .AsNoTracking()
                .Where(os => (os.DiagnosticoIniciadoEm != null && os.EnvioAprovacaoEm != null)
                          || (os.IniciadoEm != null && os.FinalizadoEm != null)
                          || (os.FinalizadoEm != null && os.EntregueEm != null))
                .Select(os => new
                {
                    os.DiagnosticoIniciadoEm,
                    os.EnvioAprovacaoEm,
                    os.IniciadoEm,
                    os.FinalizadoEm,
                    os.EntregueEm
                })
                .ToListAsync();

            var temposDiagnostico = ordens
                .Where(o => o.DiagnosticoIniciadoEm.HasValue && o.EnvioAprovacaoEm.HasValue && o.EnvioAprovacaoEm >= o.DiagnosticoIniciadoEm)
                .Select(o => (o.EnvioAprovacaoEm!.Value - o.DiagnosticoIniciadoEm!.Value).TotalHours)
                .ToList();

            var temposExecucao = ordens
                .Where(o => o.IniciadoEm.HasValue && o.FinalizadoEm.HasValue && o.FinalizadoEm >= o.IniciadoEm)
                .Select(o => (o.FinalizadoEm!.Value - o.IniciadoEm!.Value).TotalHours)
                .ToList();

            var temposFinalizacao = ordens
                .Where(o => o.FinalizadoEm.HasValue && o.EntregueEm.HasValue && o.EntregueEm >= o.FinalizadoEm)
                .Select(o => (o.EntregueEm!.Value - o.FinalizadoEm!.Value).TotalHours)
                .ToList();

            var mediaDiagnostico = temposDiagnostico.Count > 0 ? Math.Round(temposDiagnostico.Average(), 2) : 0;
            var mediaExecucao = temposExecucao.Count > 0 ? Math.Round(temposExecucao.Average(), 2) : 0;
            var mediaFinalizacao = temposFinalizacao.Count > 0 ? Math.Round(temposFinalizacao.Average(), 2) : 0;

            return new DashboardTempoMedioStatusDto(
                mediaDiagnostico,
                mediaExecucao,
                mediaFinalizacao,
                FormatarTempo(mediaDiagnostico),
                FormatarTempo(mediaExecucao),
                FormatarTempo(mediaFinalizacao),
                temposDiagnostico.Count,
                temposExecucao.Count,
                temposFinalizacao.Count);
        }

        private static string FormatarTempo(double horas)
        {
            if (horas <= 0) return "0h 0m";
            var ts = TimeSpan.FromHours(horas);
            if (ts.TotalDays >= 1)
                return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m";
            return $"{ts.Hours}h {ts.Minutes}m";
        }
    }
}
