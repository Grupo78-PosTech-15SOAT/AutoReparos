using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Shared;
using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;
using Bogus;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace AutoReparos.IntegrationTests.Endpoints;

[Collection("Integration Tests")]
public class VeiculoIntegrationTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task AutenticarClienteAsync()
    {
        var loginRequest = new LoginRequestDTO("admin@autoreparos.com", "Admin@123");

        var authResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authData = await authResponse.Content.ReadFromJsonAsync<LoginResponseDTO>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData!.Token);
    }

    private async Task<Guid> ObterIdClientePadraoAsync()
    {
        var response = await _client.GetAsync("/api/clientes?pageNumber=1&pageSize=10");
        var clientes = await response.Content.ReadFromJsonAsync<PagedResult<ClienteDTO>>();

        return clientes?.Items.FirstOrDefault()?.Id ?? throw new Exception("Cliente de teste não encontrado no banco!");
    }

    private static VeiculoCreateDTO GerarVeiculoAleatorioDto(Guid clienteId)
    {
        var faker = new Faker("pt_BR");

        var anoFabricacao = faker.Random.Int(2000, DateTime.UtcNow.Year);
        var anoModelo = faker.Random.Int(anoFabricacao, anoFabricacao + 1);

        var placa = $"{faker.Random.String2(3, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")}{faker.Random.String2(4, "0123456789")}";

        return new VeiculoCreateDTO
        (
            clienteId,
            faker.Vehicle.Manufacturer(),
            faker.Vehicle.Model(),
            anoFabricacao,
            anoModelo,
            placa,
            faker.Random.AlphaNumeric(17).ToUpper(),     
            faker.Random.ReplaceNumbers("###########")  
        );
    }

    private async Task<VeiculoDTO> CriarVeiculoAuxiliarAsync()
    {
        var clientePadraoId = await ObterIdClientePadraoAsync();

        var createDto = GerarVeiculoAleatorioDto(clientePadraoId);

        var response = await _client.PostAsJsonAsync("/api/veiculos", createDto);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            throw new Exception($"Falha ao criar veículo auxiliar: {erro}");
        }

        return (await response.Content.ReadFromJsonAsync<VeiculoDTO>())!;
    }

    [Fact(DisplayName = "POST /api/veiculos - Criar veículo válido deve retornar 201 Created")]
    public async Task CreateVeiculo_ComDadosValidos_DeveRetornarCreated()
    {
        await AutenticarClienteAsync();
        var clientePadraoId = await ObterIdClientePadraoAsync();

        var requestDto = GerarVeiculoAleatorioDto(clientePadraoId);

        var response = await _client.PostAsJsonAsync("/api/veiculos", requestDto);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Created, because: $"A API rejeitou os dados gerados: {erro}");

        var responseData = await response.Content.ReadFromJsonAsync<VeiculoDTO>();
        responseData.Should().NotBeNull();
        responseData!.Id.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /api/veiculos - Deve retornar lista paginada e 200 OK")]
    public async Task GetAllVeiculos_DeveRetornarOkEListaPaginada()
    {
        await AutenticarClienteAsync();
        await CriarVeiculoAuxiliarAsync();

        var response = await _client.GetAsync("/api/veiculos?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<PagedResult<VeiculoDTO>>();

        responseData.Should().NotBeNull();
        responseData!.Items.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "GET /api/veiculos/{id} - Veículo inexistente deve retornar 404 NotFound")]
    public async Task GetVeiculoById_QuandoNaoExiste_DeveRetornarNotFound()
    {
        await AutenticarClienteAsync();

        var response = await _client.GetAsync($"/api/veiculos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET /api/veiculos/placa/{placa} - Quando veículo existe deve retornar 200 OK")]
    public async Task GetVeiculoByPlaca_QuandoExiste_DeveRetornarOk()
    {
        await AutenticarClienteAsync();
        var veiculoCriado = await CriarVeiculoAuxiliarAsync();

        var response = await _client.GetAsync($"/api/veiculos/placa/{veiculoCriado.Placa}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<VeiculoDTO>();
        responseData.Should().NotBeNull();
        responseData!.Placa.Should().Be(veiculoCriado.Placa);
    }

    [Fact(DisplayName = "PUT /api/veiculos/{id} - Atualizar veículo deve retornar 204 NoContent")]
    public async Task UpdateVeiculo_ComDadosValidos_DeveRetornarNoContent()
    {
        await AutenticarClienteAsync();
        var veiculoCriado = await CriarVeiculoAuxiliarAsync();

        var updateDto = new VeiculoUpdateDTO("Corolla Cross", "Preto", 1995, 1999);

        var response = await _client.PutAsJsonAsync($"/api/veiculos/{veiculoCriado.Id}", updateDto);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, because: erro);
    }

    [Fact(DisplayName = "DELETE /api/veiculos/{id} - Deletar veículo deve retornar 204 NoContent")]
    public async Task DeleteVeiculo_QuandoExiste_DeveRetornarNoContent()
    {
        await AutenticarClienteAsync();
        var veiculoCriado = await CriarVeiculoAuxiliarAsync();

        var response = await _client.DeleteAsync($"/api/veiculos/{veiculoCriado.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}