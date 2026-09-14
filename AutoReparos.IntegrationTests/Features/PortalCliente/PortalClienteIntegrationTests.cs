using Amazon.Lambda.APIGatewayEvents;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.Veiculos.DTOs.Response;
using AutoReparos.AuthLambda.Functions;
using AutoReparos.AuthLambda.Models;
using AutoReparos.AuthLambda.Services;
using AutoReparos.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace AutoReparos.IntegrationTests.Features.PortalCliente;

[Collection("IntegrationTests")]
public class PortalClienteIntegrationTests(CustomWebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
    private const string JwtSecret = "test_jwt_dummy_secret_key_with_32_chars_ok!";

    private async Task<string> ObterTokenClienteAsync(string cpf, string email)
    {
        var lambda = new ClienteAuthFunction(
            new CpfValidationService(),
            new ClienteAuthDatabaseService(Factory.ConnectionString),
            new TokenGenerationService(JwtSecret, expiryHours: 1)
        );

        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new { cpf, email })
        };

        var response = await lambda.FunctionHandler(request);
        response.StatusCode.Should().Be(200);

        var responseDto = JsonSerializer.Deserialize<ClienteAuthResponse>(response.Body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return responseDto!.Token;
    }

    [Fact(DisplayName = "GET /api/clientes/meus-veiculos - Cliente deve visualizar apenas seus veículos")]
    public async Task GetMeusVeiculos_ComTokenCliente_DeveRetornarApenasVeiculosDoCliente()
    {
        // Arrange: Leandro Tavares (CPF 97632180044) possui o Gol (Placa ABC1D23)
        var token = await ObterTokenClienteAsync("97632180044", "leandro.silva@email.com");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/clientes/meus-veiculos");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var veiculos = await response.Content.ReadFromJsonAsync<List<VeiculoDto>>();

        veiculos.Should().NotBeNull();
        veiculos.Should().HaveCount(1);
        veiculos!.First().Placa.Should().Be("ABC1D23");
        veiculos.First().Modelo.Should().Be("Gol");
    }

    [Fact(DisplayName = "GET /api/ordem-servico/minhas-os - Cliente deve visualizar apenas suas ordens de serviço")]
    public async Task GetMinhasOrdensServico_ComTokenCliente_DeveRetornarApenasOrdensDoCliente()
    {
        // Arrange: Leandro Tavares
        var token = await ObterTokenClienteAsync("97632180044", "leandro.silva@email.com");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/ordem-servico/minhas-os");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ordens = await response.Content.ReadFromJsonAsync<List<MinhaOrdemServicoDto>>();

        ordens.Should().NotBeNull();
        ordens.Should().NotBeEmpty();
        ordens!.All(o => o.PlacaVeiculo == "ABC1D23").Should().BeTrue();
    }

    [Fact(DisplayName = "GET /api/ordem-servico/minhas-os?placa=XYZ2E34 - Filtrar placa de outro cliente deve retornar lista vazia (Zero-Trust)")]
    public async Task GetMinhasOrdensServico_FiltrandoPlacaDeOutroCliente_DeveRetornarListaVazia()
    {
        // Arrange: Leandro tenta consultar a placa XYZ2E34 (pertencente à Fernanda)
        var token = await ObterTokenClienteAsync("97632180044", "leandro.silva@email.com");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/ordem-servico/minhas-os?placa=XYZ2E34");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ordens = await response.Content.ReadFromJsonAsync<List<MinhaOrdemServicoDto>>();

        ordens.Should().NotBeNull();
        ordens.Should().BeEmpty(); // Proteção ativa: nenhum dado alheio vazado!
    }

    [Fact(DisplayName = "Segurança - Token de cliente deve receber 403 Forbidden ao tentar acessar rotas operacionais da oficina")]
    public async Task RotasOperacionais_ComTokenCliente_DeveRetornar403Forbidden()
    {
        // Arrange: Token do cliente Leandro
        var token = await ObterTokenClienteAsync("97632180044", "leandro.silva@email.com");

        // 1. Tentativa de listar todos os clientes da oficina
        using var reqClientes = new HttpRequestMessage(HttpMethod.Get, "/api/clientes");
        reqClientes.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respClientes = await Client.SendAsync(reqClientes);
        respClientes.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 2. Tentativa de listar kanban da oficina
        using var reqKanban = new HttpRequestMessage(HttpMethod.Get, "/api/ordem-servico/kanban");
        reqKanban.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respKanban = await Client.SendAsync(reqKanban);
        respKanban.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 3. Tentativa de acessar dashboard operacional
        using var reqDashboard = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard/metrics");
        reqDashboard.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respDashboard = await Client.SendAsync(reqDashboard);
        respDashboard.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 4. Tentativa de listar insumos
        using var reqInsumos = new HttpRequestMessage(HttpMethod.Get, "/api/insumos");
        reqInsumos.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respInsumos = await Client.SendAsync(reqInsumos);
        respInsumos.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Segurança - Requisições anônimas aos endpoints do portal devem receber 401 Unauthorized")]
    public async Task EndpointsPortal_SemToken_DeveRetornar401Unauthorized()
    {
        // 1. Meus Veículos sem token
        var respVeiculos = await Client.GetAsync("/api/clientes/meus-veiculos");
        respVeiculos.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 2. Minhas OSs sem token
        var respOrdens = await Client.GetAsync("/api/ordem-servico/minhas-os");
        respOrdens.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
