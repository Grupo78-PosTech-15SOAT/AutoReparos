using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;
using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Shared;
using Bogus;
using Bogus.Extensions.Brazil;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace AutoReparos.IntegrationTests.Endpoints;

[Collection("Integration Tests")]
public class ClienteIntegrationTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task AutenticarClienteAsync()
    {
        var loginRequest = new LoginRequestDTO("admin@autoreparos.com", "Admin@123");

        var authResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authData = await authResponse.Content.ReadFromJsonAsync<LoginResponseDTO>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData!.Token);
    }

    private static ClienteCreateDTO GerarClienteAleatorioDto()
    {
        var faker = new Faker("pt_BR");

        var cpfValido = faker.Person.Cpf();
        var telefoneValido = faker.Random.ReplaceNumbers("119########");

        return new ClienteCreateDTO(
            faker.Person.FullName,
            cpfValido,
            telefoneValido,
            faker.Internet.Email() 
        );
    }


    [Fact(DisplayName = "POST /api/clientes - Deve criar cliente e retornar 201 Created")]
    public async Task CreateCliente_ComDadosValidos_DeveRetornarCreated()
    {
        await AutenticarClienteAsync();

        var requestDto = GerarClienteAleatorioDto();

        var response = await _client.PostAsJsonAsync("/api/clientes", requestDto);

        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Created, because: $"A API rejeitou os dados: {erro}");

        var responseData = await response.Content.ReadFromJsonAsync<ClienteDTO>();
        responseData.Should().NotBeNull();
        responseData!.Id.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /api/clientes - Deve retornar a lista paginada e 200 OK")]
    public async Task GetAllClientes_DeveRetornarOkELista()
    {
        await AutenticarClienteAsync();

        var response = await _client.GetAsync("/api/clientes?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseData = await response.Content.ReadFromJsonAsync<PagedResult<ClienteDTO>>();

        responseData.Should().NotBeNull();
        responseData!.Items.Should().NotBeNull();
        responseData.Items.Should().HaveCountGreaterThan(0); 
        responseData.TotalItems.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "GET /api/clientes/{id} - Cliente inexistente deve retornar 404 NotFound")]
    public async Task GetClienteById_QuandoNaoExiste_DeveRetornarNotFound()
    {
        await AutenticarClienteAsync();
        var idInexistente = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/clientes/{idInexistente}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PUT /api/clientes/{id} - Atualizar cliente existente deve retornar 204 NoContent")]
    public async Task UpdateCliente_ComDadosValidos_DeveRetornarNoContent()
    {
        await AutenticarClienteAsync();

        var createDto = GerarClienteAleatorioDto();
        var createResponse = await _client.PostAsJsonAsync("/api/clientes", createDto);
        var clienteCriado = await createResponse.Content.ReadFromJsonAsync<ClienteDTO>();

        var updateDto = GerarClienteAleatorioDto();

        var response = await _client.PutAsJsonAsync($"/api/clientes/{clienteCriado!.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent); 
    }

    [Fact(DisplayName = "DELETE /api/clientes/{id} - Deletar cliente existente deve retornar 204 NoContent")]
    public async Task DeleteCliente_QuandoExiste_DeveRetornarNoContent()
    {
        await AutenticarClienteAsync();
        var createDto = GerarClienteAleatorioDto();
        var createResponse = await _client.PostAsJsonAsync("/api/clientes", createDto);
        var clienteCriado = await createResponse.Content.ReadFromJsonAsync<ClienteDTO>();

        var response = await _client.DeleteAsync($"/api/clientes/{clienteCriado!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent); 

        var getResponse = await _client.GetAsync($"/api/clientes/{clienteCriado.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound); 
    }
}