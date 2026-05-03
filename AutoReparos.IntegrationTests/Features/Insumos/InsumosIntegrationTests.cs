using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;
using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.DTOs.Response;
using AutoReparos.Application.Shared;
using Bogus;
using FluentAssertions;
using Xunit;

using AutoReparos.IntegrationTests.Infrastructure;

namespace AutoReparos.IntegrationTests.Features.Insumos;

public class InsumoIntegrationTests(CustomWebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory){
    private async Task<InsumoDTO> CriarInsumoAuxiliarAsync()
    {
        var faker = new Faker("pt_BR");

        var createDto = new CriarInsumoDTO
        (
            faker.Commerce.ProductName().PadRight(100).Substring(0, 50).Trim(),
            faker.Commerce.ProductDescription(),
            Math.Round(faker.Random.Decimal(10, 500), 2), 
            faker.Random.Int(10, 50) 
        );

        var response = await Client.PostAsJsonAsync("/api/insumos", createDto);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            throw new Exception($"Falha ao criar insumo auxiliar: {erro}");
        }

        return (await response.Content.ReadFromJsonAsync<InsumoDTO>())!;
    }

    [Fact(DisplayName = "POST /api/insumos - Criar insumo válido deve retornar 201 Created")]
    public async Task CreateInsumo_ComDadosValidos_DeveRetornarCreated()
    {
        await AuthenticateAsync();
        var faker = new Faker("pt_BR");

        var requestDto = new CriarInsumoDTO(faker.Commerce.ProductName(), faker.Commerce.ProductDescription(), 150.50m, 100);

        var response = await Client.PostAsJsonAsync("/api/insumos", requestDto);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Created, because: $"Erro retornado pela API: {erro}");

        var responseData = await response.Content.ReadFromJsonAsync<InsumoDTO>();
        responseData.Should().NotBeNull();
        responseData!.Id.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /api/insumos/{id} - Insumo inexistente deve retornar 404 NotFound")]
    public async Task GetInsumoById_QuandoNaoExiste_DeveRetornarNotFound()
    {
        await AuthenticateAsync();
        var response = await Client.GetAsync($"/api/insumos/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET /api/insumos - Deve retornar lista paginada e 200 OK")]
    public async Task GetAllInsumos_DeveRetornarOkEListaPaginada()
    {
        await AuthenticateAsync();
        await CriarInsumoAuxiliarAsync();

        var response = await Client.GetAsync("/api/insumos?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<PagedResult<InsumoDTO>>();

        responseData.Should().NotBeNull();
        responseData!.Items.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "PUT /api/insumos/{id} - Atualizar insumo deve retornar 204 NoContent")]
    public async Task UpdateInsumo_ComDadosValidos_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var insumoCriado = await CriarInsumoAuxiliarAsync();

        var updateDto = new AtualizarInsumoDTO("Insumo Atualizado Pelo Teste", "Nova descrição para atualização", 199.99m);

        var response = await Client.PutAsJsonAsync($"/api/insumos/{insumoCriado.Id}", updateDto);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, because: erro);
    }

    [Fact(DisplayName = "PATCH /api/insumos/{id}/adicionar-estoque - Deve retornar 204 NoContent")]
    public async Task AdicionarEstoque_ComDadosValidos_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var insumoCriado = await CriarInsumoAuxiliarAsync();

        var estoqueDto = new AtualizarEstoqueDTO(5);

        var response = await Client.PatchAsJsonAsync($"/api/insumos/{insumoCriado.Id}/adicionar-estoque", estoqueDto);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, because: erro);
    }

    [Fact(DisplayName = "PATCH /api/insumos/{id}/remover-estoque - Deve retornar 204 NoContent")]
    public async Task RemoverEstoque_ComDadosValidos_DeveRetornarNoContent()
    {
        await AuthenticateAsync();

        var insumoCriado = await CriarInsumoAuxiliarAsync();

        var estoqueDto = new AtualizarEstoqueDTO(5);
        var response = await Client.PatchAsJsonAsync($"/api/insumos/{insumoCriado.Id}/remover-estoque", estoqueDto);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, because: erro);
    }

    [Fact(DisplayName = "DELETE /api/insumos/{id} - Deletar insumo deve retornar 204 NoContent")]
    public async Task DeleteInsumo_QuandoExiste_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var insumoCriado = await CriarInsumoAuxiliarAsync();

        var response = await Client.DeleteAsync($"/api/insumos/{insumoCriado.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}