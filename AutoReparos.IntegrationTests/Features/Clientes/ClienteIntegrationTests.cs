using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Shared;
using AutoReparos.IntegrationTests.Infrastructure;
using Bogus;
using Bogus.Extensions.Brazil;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AutoReparos.IntegrationTests.Features.Clientes;

public class ClienteIntegrationTests(CustomWebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
    private static ClienteCreateDto GerarClienteAleatorioDto()
    {
        var faker = new Faker("pt_BR");

        var cpfValido = faker.Person.Cpf();
        var telefoneValido = faker.Random.ReplaceNumbers("119########");

        return new ClienteCreateDto(
            faker.Person.FullName,
            cpfValido,
            telefoneValido,
            faker.Internet.Email()
        );
    }


    [Fact(DisplayName = "POST /api/clientes - Deve criar cliente e retornar 201 Created")]
    public async Task CreateCliente_ComDadosValidos_DeveRetornarCreated()
    {
        await AuthenticateAsync();

        var requestDto = GerarClienteAleatorioDto();

        var response = await Client.PostAsJsonAsync("/api/clientes", requestDto);

        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Created, because: $"A API rejeitou os dados: {erro}");

        var responseData = await response.Content.ReadFromJsonAsync<ClienteDto>();
        responseData.Should().NotBeNull();
        responseData!.Id.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /api/clientes - Deve retornar a lista paginada e 200 OK")]
    public async Task GetAllClientes_DeveRetornarOkELista()
    {
        await AuthenticateAsync();

        var response = await Client.GetAsync("/api/clientes?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseData = await response.Content.ReadFromJsonAsync<PagedResult<ClienteDto>>();

        responseData.Should().NotBeNull();
        responseData!.Items.Should().NotBeNull();
        responseData.Items.Should().HaveCountGreaterThan(0);
        responseData.TotalItems.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "GET /api/clientes/{id} - Cliente inexistente deve retornar 404 NotFound")]
    public async Task GetClienteById_QuandoNaoExiste_DeveRetornarNotFound()
    {
        await AuthenticateAsync();
        var idInexistente = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/clientes/{idInexistente}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PUT /api/clientes/{id} - Atualizar cliente existente deve retornar 204 NoContent")]
    public async Task UpdateCliente_ComDadosValidos_DeveRetornarNoContent()
    {
        await AuthenticateAsync();

        var createDto = GerarClienteAleatorioDto();
        var createResponse = await Client.PostAsJsonAsync("/api/clientes", createDto);
        var clienteCriado = await createResponse.Content.ReadFromJsonAsync<ClienteDto>();

        var updateDto = GerarClienteAleatorioDto();

        var response = await Client.PutAsJsonAsync($"/api/clientes/{clienteCriado!.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "DELETE /api/clientes/{id} - Deletar cliente existente deve retornar 204 NoContent")]
    public async Task DeleteCliente_QuandoExiste_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var createDto = GerarClienteAleatorioDto();
        var createResponse = await Client.PostAsJsonAsync("/api/clientes", createDto);
        var clienteCriado = await createResponse.Content.ReadFromJsonAsync<ClienteDto>();

        var response = await Client.DeleteAsync($"/api/clientes/{clienteCriado!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/clientes/{clienteCriado.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}