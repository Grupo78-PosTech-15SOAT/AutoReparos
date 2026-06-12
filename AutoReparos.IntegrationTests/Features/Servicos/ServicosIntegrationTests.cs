using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Shared;
using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;
using AutoReparos.IntegrationTests.Infrastructure;
using Bogus;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AutoReparos.IntegrationTests.Features.Servicos;

public class ServicoIntegrationTests(CustomWebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
    private async Task<ServicoDto> CriarServicoAuxiliarAsync()
    {
        var faker = new Faker("pt_BR");

        var createDto = new CriarServicoDto
        (
            faker.Commerce.ProductName(),
            faker.Commerce.ProductDescription(),
            Math.Round(faker.Random.Decimal(50, 1000), 2)
        );

        var response = await Client.PostAsJsonAsync("/api/servicos", createDto);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            throw new Exception($"Falha ao criar serviço auxiliar: {erro}");
        }

        return (await response.Content.ReadFromJsonAsync<ServicoDto>())!;
    }
    private async Task<(Guid ClienteId, Guid VeiculoId, Guid ServicoId)> ObterIdsDependenciasAsync()
    {
        var faker = new Faker("pt_BR");

        var resCliente = await Client.GetFromJsonAsync<PagedResult<ClienteDto>>("/api/clientes?pageNumber=1&pageSize=1");
        var clienteId = resCliente!.Items.First().Id;

        var requestDto = new VeiculoCreateDto(
            clienteId,
            "Toyota",
            "Corolla",
            2020,
            2022,
            "ABC-1234",
            "8Wz32v68wN7vf6617",
            "38579863163"
        );
        await Client.PostAsJsonAsync("/api/veiculos", requestDto);
        var resVeiculo = await Client.GetFromJsonAsync<PagedResult<VeiculoDto>>($"/api/veiculos?clienteId={clienteId}&pageNumber=1&pageSize=1");
        var veiculoId = resVeiculo!.Items.First().Id;

        var createDto = new CriarServicoDto
       (
           faker.Commerce.ProductName(),
           faker.Commerce.ProductDescription(),
           Math.Round(faker.Random.Decimal(50, 1000), 2)
       );

        await Client.PostAsJsonAsync("/api/servicos", createDto);
        var resServico = await Client.GetFromJsonAsync<PagedResult<ServicoDto>>("/api/servicos?pageNumber=1&pageSize=1");
        var servicoId = resServico!.Items.First().Id;

        return (clienteId, veiculoId, servicoId);
    }


    [Fact(DisplayName = "POST /api/servicos - Criar serviço válido deve retornar 201 Created")]
    public async Task CreateServico_ComDadosValidos_DeveRetornarCreated()
    {
        await AuthenticateAsync();

        var requestDto = new CriarServicoDto(
            "Troca de Óleo",
            "Troca completa do óleo do motor e filtro",
            150.00m
        );

        var response = await Client.PostAsJsonAsync("/api/servicos", requestDto);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Created, because: $"Erro retornado pela API: {erro}");

        var responseData = await response.Content.ReadFromJsonAsync<ServicoDto>();
        responseData.Should().NotBeNull();
        responseData!.Id.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /api/servicos - Deve retornar lista paginada e 200 OK")]
    public async Task GetAllServicos_DeveRetornarOkEListaPaginada()
    {
        await AuthenticateAsync();
        await CriarServicoAuxiliarAsync();

        var response = await Client.GetAsync("/api/servicos?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<PagedResult<ServicoDto>>();

        responseData.Should().NotBeNull();
        responseData!.Items.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "GET /api/servicos/{id} - Serviço inexistente deve retornar 404 NotFound")]
    public async Task GetServicoById_QuandoNaoExiste_DeveRetornarNotFound()
    {
        await AuthenticateAsync();

        var response = await Client.GetAsync($"/api/servicos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET /api/servicos/{id} - Quando serviço existe deve retornar 200 OK")]
    public async Task GetServicoById_QuandoExiste_DeveRetornarOk()
    {
        await AuthenticateAsync();
        var servicoCriado = await CriarServicoAuxiliarAsync();

        var response = await Client.GetAsync($"/api/servicos/{servicoCriado.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<ServicoDto>();
        responseData.Should().NotBeNull();
        responseData!.Id.Should().Be(servicoCriado.Id);
    }

    [Fact(DisplayName = "GET /api/servicos/tempo-medio - Deve retornar lista de tempo médio e 200 OK")]
    public async Task GetTempoMedioServicos_DeveRetornarOk()
    {
        await AuthenticateAsync();
        await CriarServicoAuxiliarAsync();

        var response = await Client.GetAsync("/api/servicos/tempo-medio");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<IEnumerable<TempoMedioServicoDto>>();
        responseData.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /api/servicos/{id}/tempo-medio - Quando serviço não tem histórico, deve retornar 404 NotFound")]
    public async Task GetTempoMedioServicoById_QuandoSemHistorico_DeveRetornarNotFound()
    {
        await AuthenticateAsync();
        var servicoCriado = await CriarServicoAuxiliarAsync();

        var response = await Client.GetAsync($"/api/servicos/{servicoCriado.Id}/tempo-medio");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET /api/servicos/{id}/tempo-medio - Quando serviço possui histórico, deve retornar 200 OK")]
    public async Task GetTempoMedioServicoById_QuandoComHistorico_DeveRetornarOk()
    {
        await AuthenticateAsync();

        var deps = await ObterIdsDependenciasAsync();

        var criarOsDto = new CriarOrdemServicoDto(deps.ClienteId, deps.VeiculoId, "Teste para forçar tempo médio");
        var resOs = await Client.PostAsJsonAsync("/api/ordens-servico", criarOsDto);
        var osCriada = await resOs.Content.ReadFromJsonAsync<OrdemServicoDto>();

        await Client.PatchAsync($"/api/ordens-servico/{osCriada!.Id}/iniciar-diagnostico", null);

        await Client.PostAsJsonAsync($"/api/ordens-servico/{osCriada.Id}/servicos", new
        {
            ServicoId = deps.ServicoId,
            ValorCobrado = 150.00m
        });

        await Client.PatchAsync($"/api/ordens-servico/{osCriada.Id}/enviar-para-aprovacao", null);
        await Client.PatchAsync($"/api/ordens-servico/{osCriada.Id}/aprovar", null);

        var osDetalhe = await Client.GetFromJsonAsync<OrdemServicoDetalheDto>($"/api/ordens-servico/{osCriada.Id}");
        var osServicoId = osDetalhe!.Servicos.First().Id;

        await Client.PatchAsync($"/api/ordens-servico/{osCriada.Id}/servicos/{osServicoId}/iniciar", null);

        await Task.Delay(100);

        await Client.PatchAsync($"/api/ordens-servico/{osCriada.Id}/servicos/{osServicoId}/concluir", null);

        var response = await Client.GetAsync($"/api/servicos/{deps.ServicoId}/tempo-medio");
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: erro);

        var responseData = await response.Content.ReadFromJsonAsync<TempoMedioServicoDto>();
        responseData.Should().NotBeNull();
    }

    [Fact(DisplayName = "PUT /api/servicos/{id} - Atualizar serviço deve retornar 204 NoContent")]
    public async Task UpdateServico_ComDadosValidos_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var servicoCriado = await CriarServicoAuxiliarAsync();

        var updateDto = new AtualizarServicoDto(
            "Troca de Óleo Premium",
            "Nova descrição com óleo sintético",
            250.00m
        );

        var response = await Client.PutAsJsonAsync($"/api/servicos/{servicoCriado.Id}", updateDto);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, because: erro);
    }

    [Fact(DisplayName = "DELETE /api/servicos/{id} - Deletar serviço deve retornar 204 NoContent")]
    public async Task DeleteServico_QuandoExiste_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var servicoCriado = await CriarServicoAuxiliarAsync();
        var response = await Client.DeleteAsync($"/api/servicos/{servicoCriado.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}