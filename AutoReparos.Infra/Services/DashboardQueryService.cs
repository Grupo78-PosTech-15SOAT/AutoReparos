using AutoReparos.Application.Dashboard.DTOs;
using AutoReparos.Application.Dashboard.Services;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoReparos.Infra.Services
{
    public class DashboardQueryService(AppDbContext context) : IDashboardQueryService
    {
        private const int EstoqueMinimo = 5;

        public async Task<DashboardMetricsDto> GetMetricsAsync()
        {
            var ordens = context.OrdensServico.AsNoTracking();

            var totalOrdensServico = await ordens.CountAsync();
            var ordensEmExecucao = await ordens.CountAsync(os => os.Status == EStatusOrdemServico.EmExecucao);

            var valoresFaturados = await ordens
                .Where(os => os.Status == EStatusOrdemServico.Finalizada || os.Status == EStatusOrdemServico.Entregue)
                .Select(os => os.Servicos.Sum(s => s.ValorCobrado) + os.Insumos.Sum(i => i.ValorTotal))
                .ToListAsync();

            var ultimasOrdens = await (
                from os in ordens
                join veiculo in context.Veiculos.AsNoTracking() on os.VeiculoId equals veiculo.Id
                orderby os.CriadoEm descending
                select new DashboardOrdemServicoDto(
                    os.Id,
                    os.Status,
                    os.Servicos.Sum(s => s.ValorCobrado) + os.Insumos.Sum(i => i.ValorTotal),
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
                valoresFaturados.Sum(),
                ordensEmExecucao,
                totalOrdensServico,
                ultimasOrdens,
                insumosCriticos);
        }
    }
}
