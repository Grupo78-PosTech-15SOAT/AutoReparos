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

            return new DashboardMetricsDto(
                faturamentoMesAtual,
                faturamentoMesAnterior,
                ordensEmExecucao,
                totalOrdensMesAtual,
                ultimasOrdens,
                insumosCriticos,
                historico);
        }
    }
}
