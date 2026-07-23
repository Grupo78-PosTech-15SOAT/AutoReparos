using AutoReparos.Application.Dashboard.DTOs;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.ValueObjects;
using AutoReparos.Infra.Data;
using AutoReparos.Infra.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AutoReparos.Application.Tests.Dashboard
{
    public class DashboardQueryServiceTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private (List<Cliente> clientes, List<Veiculo> veiculos) SeedBaseEntities(AppDbContext context)
        {
            var cliente1 = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var cliente2 = new Cliente("Maria Santos", "66265464060", "11888888888", "maria@teste.com");
            var cliente3 = new Cliente("Pedro Souza", "99806446054", "11777777777", "pedro@teste.com");
            context.Clientes.AddRange(cliente1, cliente2, cliente3);

            var veiculo1 = new Veiculo(
                cliente1.Id,
                "Chevrolet",
                "Onix",
                2020,
                2021,
                new Placa("ABC1D23"),
                new Chassi("9BD111060T5002156"),
                new Renavam("00123456789"));

            var veiculo2 = new Veiculo(
                cliente2.Id,
                "Fiat",
                "Uno",
                2018,
                2019,
                new Placa("DEF5G67"),
                new Chassi("9BD111060T5002157"),
                new Renavam("00123456780"));

            var veiculo3 = new Veiculo(
                cliente3.Id,
                "Volkswagen",
                "Gol",
                2015,
                2016,
                new Placa("GHI9J01"),
                new Chassi("9BD111060T5002158"),
                new Renavam("00123456781"));

            context.Veiculos.AddRange(veiculo1, veiculo2, veiculo3);
            context.SaveChanges();

            return (new List<Cliente> { cliente1, cliente2, cliente3 }, new List<Veiculo> { veiculo1, veiculo2, veiculo3 });
        }

        private OrdemServico CreateOrdemServicoWithStatus(
            Guid clienteId,
            Guid veiculoId,
            EStatusOrdemServico status,
            DateTime criadoEm,
            List<(Guid servicoId, decimal valorCobrado)> servicosInfo,
            List<(Guid? insumoId, string descricao, decimal valorUnitario, int quantidade)> insumosInfo)
        {
            // Use the new internal constructor that sets CriadoEm and Status without Reflection
            var os = new OrdemServico(clienteId, veiculoId, "Test OS", criadoEm, EStatusOrdemServico.Recebida);

            // Add services
            foreach (var s in servicosInfo)
            {
                var osServico = new OrdemServicoServico(os.Id, s.servicoId, s.valorCobrado);
                os.AdicionarServico(osServico);
            }

            // Add insumos
            foreach (var i in insumosInfo)
            {
                var osInsumo = new OrdemServicoInsumo(
                    os.Id,
                    i.insumoId,
                    i.descricao,
                    i.valorUnitario,
                    i.quantidade,
                    i.insumoId.HasValue ? EOrigemInsumo.Estoque : EOrigemInsumo.CompraEspecifica);
                os.AdicionarInsumo(osInsumo);
            }

            // Perform transitions using public domain methods to achieve the desired target status
            if (status == EStatusOrdemServico.EmDiagnostico)
            {
                os.IniciarDiagnostico();
            }
            else if (status == EStatusOrdemServico.AguardandoAprovacao)
            {
                os.IniciarDiagnostico();
                os.AguardarAprovacao();
            }
            else if (status == EStatusOrdemServico.EmExecucao)
            {
                os.IniciarDiagnostico();
                os.AguardarAprovacao();
                os.Aprovar();
            }
            else if (status == EStatusOrdemServico.Finalizada)
            {
                os.IniciarDiagnostico();
                os.AguardarAprovacao();
                os.Aprovar();
                foreach (var servico in os.Servicos)
                {
                    os.IniciarServico(servico.Id);
                    os.ConcluirServico(servico.Id);
                }
            }
            else if (status == EStatusOrdemServico.Entregue)
            {
                os.IniciarDiagnostico();
                os.AguardarAprovacao();
                os.Aprovar();
                foreach (var servico in os.Servicos)
                {
                    os.IniciarServico(servico.Id);
                    os.ConcluirServico(servico.Id);
                }
                os.Entregar();
            }

            return os;
        }

        [Fact]
        public async Task GetMetricsAsync_ShouldReturnCorrectFaturamento_OnlySummingFinishedAndDelivered()
        {
            // Arrange
            using var context = CreateDbContext();
            var (clientes, veiculos) = SeedBaseEntities(context);

            var servicoDummyId = Guid.NewGuid();

            // 1. OS with Status = Finalizada (Should sum: 100.00 service + 2 * 50.00 insumo = 200.00)
            var osFinalizada = CreateOrdemServicoWithStatus(
                clientes[0].Id,
                veiculos[0].Id,
                EStatusOrdemServico.Finalizada,
                DateTime.UtcNow,
                [(servicoDummyId, 100.00m)],
                [(Guid.NewGuid(), "Insumo 1", 50.00m, 2)]);

            // 2. OS with Status = Entregue (Should sum: 150.00 service + 1 * 30.00 insumo = 180.00)
            var osEntregue = CreateOrdemServicoWithStatus(
                clientes[1].Id,
                veiculos[1].Id,
                EStatusOrdemServico.Entregue,
                DateTime.UtcNow,
                [(servicoDummyId, 150.00m)],
                [(Guid.NewGuid(), "Insumo 2", 30.00m, 1)]);

            // 3. OS with Status = Recebida (Should NOT sum: 500.00 service + 1 * 100.00 insumo = 600.00)
            var osRecebida = CreateOrdemServicoWithStatus(
                clientes[2].Id,
                veiculos[2].Id,
                EStatusOrdemServico.Recebida,
                DateTime.UtcNow,
                [(servicoDummyId, 500.00m)],
                [(Guid.NewGuid(), "Insumo 3", 100.00m, 1)]);

            // 4. OS with Status = EmDiagnostico (Should NOT sum: 300.00 service)
            var osEmDiagnostico = CreateOrdemServicoWithStatus(
                clientes[0].Id,
                veiculos[0].Id,
                EStatusOrdemServico.EmDiagnostico,
                DateTime.UtcNow,
                [(servicoDummyId, 300.00m)],
                []);

            // 5. OS with Status = AguardandoAprovacao (Should NOT sum: 400.00 service)
            var osAguardandoAprovacao = CreateOrdemServicoWithStatus(
                clientes[1].Id,
                veiculos[1].Id,
                EStatusOrdemServico.AguardandoAprovacao,
                DateTime.UtcNow,
                [(servicoDummyId, 400.00m)],
                []);

            // 6. OS with Status = EmExecucao (Should NOT sum: 200.00 service)
            var osEmExecucao = CreateOrdemServicoWithStatus(
                clientes[2].Id,
                veiculos[2].Id,
                EStatusOrdemServico.EmExecucao,
                DateTime.UtcNow,
                [(servicoDummyId, 200.00m)],
                []);

            context.OrdensServico.AddRange(osFinalizada, osEntregue, osRecebida, osEmDiagnostico, osAguardandoAprovacao, osEmExecucao);
            await context.SaveChangesAsync();

            var service = new DashboardQueryService(context);

            // Act
            var result = await service.GetMetricsAsync();

            // Assert
            // Expected: 200.00 + 180.00 = 380.00
            result.FaturamentoMesAtual.Should().Be(380.00m);
        }

        [Fact]
        public async Task GetMetricsAsync_ShouldReturnCorrectCounts_ForExecutingAndTotalOSs()
        {
            // Arrange
            using var context = CreateDbContext();
            var (clientes, veiculos) = SeedBaseEntities(context);
            var servicoDummyId = Guid.NewGuid();

            var os1 = CreateOrdemServicoWithStatus(clientes[0].Id, veiculos[0].Id, EStatusOrdemServico.Recebida, DateTime.UtcNow, [(servicoDummyId, 100.00m)], []);
            var os2 = CreateOrdemServicoWithStatus(clientes[1].Id, veiculos[1].Id, EStatusOrdemServico.EmDiagnostico, DateTime.UtcNow, [(servicoDummyId, 100.00m)], []);
            var os3 = CreateOrdemServicoWithStatus(clientes[2].Id, veiculos[2].Id, EStatusOrdemServico.AguardandoAprovacao, DateTime.UtcNow, [(servicoDummyId, 100.00m)], []);
            var os4 = CreateOrdemServicoWithStatus(clientes[0].Id, veiculos[0].Id, EStatusOrdemServico.EmExecucao, DateTime.UtcNow, [(servicoDummyId, 100.00m)], []);
            var os5 = CreateOrdemServicoWithStatus(clientes[1].Id, veiculos[1].Id, EStatusOrdemServico.Finalizada, DateTime.UtcNow, [(servicoDummyId, 100.00m)], []);
            var os6 = CreateOrdemServicoWithStatus(clientes[2].Id, veiculos[2].Id, EStatusOrdemServico.Entregue, DateTime.UtcNow, [(servicoDummyId, 100.00m)], []);

            context.OrdensServico.AddRange(os1, os2, os3, os4, os5, os6);
            await context.SaveChangesAsync();

            var service = new DashboardQueryService(context);

            // Act
            var result = await service.GetMetricsAsync();

            // Assert
            result.TotalOrdensMesAtual.Should().Be(6);
            result.OrdensEmExecucao.Should().Be(1);
        }

        [Fact]
        public async Task GetMetricsAsync_ShouldReturnLast5OSsSortedByDateDescending()
        {
            // Arrange
            using var context = CreateDbContext();
            var (clientes, veiculos) = SeedBaseEntities(context);
            var servicoDummyId = Guid.NewGuid();

            var baseDate = DateTime.UtcNow;

            // Seed 6 OSs with sequential dates
            var os1 = CreateOrdemServicoWithStatus(clientes[0].Id, veiculos[0].Id, EStatusOrdemServico.Recebida, baseDate.AddDays(-6), [(servicoDummyId, 10.00m)], []);
            var os2 = CreateOrdemServicoWithStatus(clientes[1].Id, veiculos[1].Id, EStatusOrdemServico.Recebida, baseDate.AddDays(-5), [(servicoDummyId, 20.00m)], []);
            var os3 = CreateOrdemServicoWithStatus(clientes[2].Id, veiculos[2].Id, EStatusOrdemServico.Recebida, baseDate.AddDays(-4), [(servicoDummyId, 30.00m)], []);
            var os4 = CreateOrdemServicoWithStatus(clientes[0].Id, veiculos[0].Id, EStatusOrdemServico.Recebida, baseDate.AddDays(-3), [(servicoDummyId, 40.00m)], []);
            var os5 = CreateOrdemServicoWithStatus(clientes[1].Id, veiculos[1].Id, EStatusOrdemServico.Recebida, baseDate.AddDays(-2), [(servicoDummyId, 50.00m)], []);
            var os6 = CreateOrdemServicoWithStatus(clientes[2].Id, veiculos[2].Id, EStatusOrdemServico.Recebida, baseDate.AddDays(-1), [(servicoDummyId, 60.00m)], []);

            context.OrdensServico.AddRange(os1, os2, os3, os4, os5, os6);
            await context.SaveChangesAsync();

            var service = new DashboardQueryService(context);

            // Act
            var result = await service.GetMetricsAsync();

            // Assert
            result.UltimasOrdens.Should().HaveCount(5);
            
            // Expected order: os6 (1 day ago), os5 (2 days ago), os4 (3 days ago), os3 (4 days ago), os2 (5 days ago).
            // os1 (6 days ago) should not be returned.
            var ordensList = result.UltimasOrdens.ToList();
            
            ordensList[0].Id.Should().Be(os6.Id);
            ordensList[0].ValorTotal.Should().Be(60.00m);
            ordensList[0].ModeloVeiculo.Should().Be(veiculos[2].Modelo);
            ordensList[0].PlacaVeiculo.Should().Be(veiculos[2].Placa.Valor);

            ordensList[1].Id.Should().Be(os5.Id);
            ordensList[1].ValorTotal.Should().Be(50.00m);
            ordensList[1].ModeloVeiculo.Should().Be(veiculos[1].Modelo);
            ordensList[1].PlacaVeiculo.Should().Be(veiculos[1].Placa.Valor);

            ordensList[2].Id.Should().Be(os4.Id);
            ordensList[2].ValorTotal.Should().Be(40.00m);
            ordensList[2].ModeloVeiculo.Should().Be(veiculos[0].Modelo);
            ordensList[2].PlacaVeiculo.Should().Be(veiculos[0].Placa.Valor);

            ordensList[3].Id.Should().Be(os3.Id);
            ordensList[3].ValorTotal.Should().Be(30.00m);
            ordensList[3].ModeloVeiculo.Should().Be(veiculos[2].Modelo);
            ordensList[3].PlacaVeiculo.Should().Be(veiculos[2].Placa.Valor);

            ordensList[4].Id.Should().Be(os2.Id);
            ordensList[4].ValorTotal.Should().Be(20.00m);
            ordensList[4].ModeloVeiculo.Should().Be(veiculos[1].Modelo);
            ordensList[4].PlacaVeiculo.Should().Be(veiculos[1].Placa.Valor);
        }

        [Fact]
        public async Task GetMetricsAsync_ShouldIdentifyCriticalStockItemsCorrectly_IncludingLimitCases()
        {
            // Arrange
            using var context = CreateDbContext();

            // Insumos:
            // A: stock 4 (critical)
            // B: stock 5 (critical - limit case)
            // C: stock 6 (not critical - limit case)
            // D: stock 10 (not critical)
            // E: stock 5, name "Zeta"
            // F: stock 5, name "Alpha" (to test alphabetical order sorting for identical stocks)
            var insumoA = new Insumo("Insumo A", "Desc", 10.00m, 4);
            var insumoB = new Insumo("Insumo B", "Desc", 10.00m, 5);
            var insumoC = new Insumo("Insumo C", "Desc", 10.00m, 6);
            var insumoD = new Insumo("Insumo D", "Desc", 10.00m, 10);
            var insumoZeta = new Insumo("Zeta", "Desc", 10.00m, 5);
            var insumoAlpha = new Insumo("Alpha", "Desc", 10.00m, 5);

            context.Insumos.AddRange(insumoA, insumoB, insumoC, insumoD, insumoZeta, insumoAlpha);
            await context.SaveChangesAsync();

            var service = new DashboardQueryService(context);

            // Act
            var result = await service.GetMetricsAsync();

            // Assert
            // Expected critical: insumoA (4), insumoAlpha (5), insumoB (5), insumoZeta (5).
            // Total: 4 critical items.
            // Sorted by Stock (asc), then by Name (asc).
            result.InsumosCriticos.Should().HaveCount(4);

            var criticosList = result.InsumosCriticos.ToList();

            criticosList[0].Id.Should().Be(insumoA.Id);
            criticosList[0].QuantidadeEstoque.Should().Be(4);

            // Stock 5, sorted alphabetically: Alpha, Insumo B, Zeta
            criticosList[1].Id.Should().Be(insumoAlpha.Id);
            criticosList[1].Nome.Should().Be("Alpha");

            criticosList[2].Id.Should().Be(insumoB.Id);
            criticosList[2].Nome.Should().Be("Insumo B");

            criticosList[3].Id.Should().Be(insumoZeta.Id);
            criticosList[3].Nome.Should().Be("Zeta");
        }

        [Fact]
        public async Task GetMetricsAsync_ShouldReturnEmptyMetrics_WhenDatabaseIsEmpty()
        {
            // Arrange
            using var context = CreateDbContext();
            var service = new DashboardQueryService(context);

            // Act
            var result = await service.GetMetricsAsync();

            // Assert
            result.Should().NotBeNull();
            result.FaturamentoMesAtual.Should().Be(0m);
            result.FaturamentoMesAnterior.Should().Be(0m);
            result.OrdensEmExecucao.Should().Be(0);
            result.TotalOrdensMesAtual.Should().Be(0);
            result.UltimasOrdens.Should().BeEmpty();
            result.InsumosCriticos.Should().BeEmpty();
            result.HistoricoMensal.Should().NotBeEmpty();
        }
    }
}
