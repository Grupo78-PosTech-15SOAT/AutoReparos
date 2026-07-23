using AutoReparos.Application.Dashboard.DTOs;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.ValueObjects;
using AutoReparos.Infra.Data;
using AutoReparos.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AutoReparos.IntegrationTests.Features.Dashboard;

public class DashboardIntegrationTests(CustomWebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
    [Fact(DisplayName = "GET /api/dashboard/metrics - Without authorization should return 401 Unauthorized")]
    public async Task GetMetrics_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/dashboard/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /api/dashboard/metrics - With invalid token should return 401 Unauthorized")]
    public async Task GetMetrics_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token-format-or-key");

        // Act
        var response = await Client.GetAsync("/api/dashboard/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /api/dashboard/metrics - With authorization and valid seed data should return 200 OK and correct metrics")]
    public async Task GetMetrics_WithAuth_ShouldReturnOkWithMetrics()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/dashboard/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var metrics = await response.Content.ReadFromJsonAsync<DashboardMetricsDto>();
        metrics.Should().NotBeNull();

        // Asserting default values from the seeded database:
        // totalOrdensServico = 3
        // ordensEmExecucao = 1 (OS1 is EmExecucao, OS2 is AguardandoAprovacao, OS3 is Recebida)
        // faturamentoTotal = 0 (since no OS has status Finalizada or Entregue)
        metrics!.TotalOrdensMesAtual.Should().Be(3);
        metrics.OrdensEmExecucao.Should().Be(1);
        metrics.FaturamentoMesAtual.Should().Be(0);
        metrics.FaturamentoMesAnterior.Should().Be(0);
        metrics.HistoricoMensal.Should().NotBeEmpty();

        // UltimasOrdens should list the seeded ordens (up to 5, we have 3)
        metrics.UltimasOrdens.Should().HaveCount(3);
        foreach (var ord in metrics.UltimasOrdens)
        {
            ord.Id.Should().NotBeEmpty();
            ord.Status.Should().BeDefined();
            ord.ModeloVeiculo.Should().NotBeNullOrWhiteSpace();
            ord.PlacaVeiculo.Should().NotBeNullOrWhiteSpace();
            ord.ValorTotal.Should().BeGreaterThanOrEqualTo(0);
        }

        // InsumosCriticos should be empty since all seeded insumos have Quantity > 5
        metrics.InsumosCriticos.Should().BeEmpty();
    }

    [Fact(DisplayName = "GET /api/dashboard/metrics - With completed/delivered OS in PostgreSQL should return correct faturamento and detailed lists")]
    public async Task GetMetrics_WithCompletedOrdemServico_ShouldCalculateFaturamentoAndReturnDetailedLists()
    {
        // Arrange
        await AuthenticateAsync();

        // 1. Get database context to seed our test data
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Seed clients & vehicles
            var cliente = new Cliente("Test Client Integration", "22510257072", "11988887777", "testint@email.com");
            context.Clientes.Add(cliente);

            var veiculo = new Veiculo(
                cliente.Id,
                "Ford",
                "Ka",
                2019,
                2020,
                new Placa("XYZ9A87"),
                new Chassi("9BD111060T5002199"),
                new Renavam("98765432101"));
            context.Veiculos.Add(veiculo);

            var insumo = new Insumo("Filtro de Ar de Cabine", "Filtro", 50.00m, 4); // Stock 4 -> critical!
            context.Insumos.Add(insumo);

            var servico = new Servico("Higienização do Ar Condicionado", "Higienizacao", 150.00m);
            context.Servicos.Add(servico);

            await context.SaveChangesAsync();

            // 2. Create and transition Ordem de Serviço to Entregue (or Finalizada)
            var os = new OrdemServico(cliente.Id, veiculo.Id, "Integration OS");
            await context.OrdensServico.AddAsync(os);
            await context.SaveChangesAsync();

            var osServico = new OrdemServicoServico(os.Id, servico.Id, 150.00m);
            os.AdicionarServico(osServico);

            var osInsumo = new OrdemServicoInsumo(os.Id, insumo.Id, insumo.Nome, insumo.Valor, 2, EOrigemInsumo.Estoque);
            os.AdicionarInsumo(osInsumo);

            os.IniciarDiagnostico();
            os.AguardarAprovacao();
            os.Aprovar();
            os.IniciarServico(osServico.Id);
            os.ConcluirServico(osServico.Id); // Transitions to Finalizada
            os.Entregar(); // Transitions to Entregue

            await context.SaveChangesAsync();
        }

        // Act
        var response = await Client.GetAsync("/api/dashboard/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var metrics = await response.Content.ReadFromJsonAsync<DashboardMetricsDto>();
        metrics.Should().NotBeNull();

        // FaturamentoMesAtual:
        // Prior to this test, the seeded database has 3 OSs in progress (Faturamento = 0).
        // Our new OS has 150.00 (service) + 2 * 50.00 (insumo) = 250.00.
        // It's Entregue, so FaturamentoMesAtual should be 250.00m.
        metrics!.FaturamentoMesAtual.Should().Be(250.00m);
        metrics.TotalOrdensMesAtual.Should().Be(4); // 3 seeded + 1 new

        // Assert on lists (UltimasOrdens)
        metrics.UltimasOrdens.Should().NotBeEmpty();
        var latestOs = metrics.UltimasOrdens.First(o => o.ModeloVeiculo == "Ka");
        latestOs.Status.Should().Be(EStatusOrdemServico.Entregue);
        latestOs.ValorTotal.Should().Be(250.00m);
        latestOs.PlacaVeiculo.Should().Be("XYZ9A87");
        latestOs.ModeloVeiculo.Should().Be("Ka");
        latestOs.CriadoEm.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));

        // Assert on lists (InsumosCriticos)
        // Since we seeded an insumo with stock = 4, it should be in the critical list.
        metrics.InsumosCriticos.Should().NotBeEmpty();
        var criticalInsumo = metrics.InsumosCriticos.First(i => i.Nome == "Filtro de Ar de Cabine");
        criticalInsumo.QuantidadeEstoque.Should().Be(4);
        criticalInsumo.Nome.Should().Be("Filtro de Ar de Cabine");
    }

    [Fact(DisplayName = "GET /api/dashboard/metrics - With empty database should return 200 OK with zeroed metrics")]
    public async Task GetMetrics_WithEmptyDatabase_ShouldReturnOkWithZeroedMetrics()
    {
        // Arrange
        await AuthenticateAsync(); // Get valid admin token first
        await Factory.ResetDatabaseAsync(); // Clear all seeded data so the database is completely empty

        // Act
        var response = await Client.GetAsync("/api/dashboard/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var metrics = await response.Content.ReadFromJsonAsync<DashboardMetricsDto>();
        metrics.Should().NotBeNull();

        metrics!.TotalOrdensMesAtual.Should().Be(0);
        metrics.OrdensEmExecucao.Should().Be(0);
        metrics.FaturamentoMesAtual.Should().Be(0);
        metrics.FaturamentoMesAnterior.Should().Be(0);
        metrics.HistoricoMensal.Should().NotBeEmpty();
        metrics.UltimasOrdens.Should().BeEmpty();
        metrics.InsumosCriticos.Should().BeEmpty();
    }
}
