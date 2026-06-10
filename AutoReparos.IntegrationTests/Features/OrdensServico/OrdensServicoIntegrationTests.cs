using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Shared;
using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.IntegrationTests.Infrastructure;
using Bogus;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AutoReparos.IntegrationTests.Features.OrdensServico;

public class OrdemServicoIntegrationTests(CustomWebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
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

    private async Task<OrdemServicoDto> CriarOrdemServicoAuxiliarAsync()
    {
        var deps = await ObterIdsDependenciasAsync();
        var faker = new Faker();

        var createDto = new CriarOrdemServicoDto(deps.ClienteId, deps.VeiculoId, faker.Lorem.Sentence());
        var response = await Client.PostAsJsonAsync("/api/ordens-servico", createDto);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Falha ao criar OS auxiliar.");

        return (await response.Content.ReadFromJsonAsync<OrdemServicoDto>())!;
    }

    private async Task<(Guid OsId, Guid OsServicoId)> PrepararOsEmExecucaoAsync()
    {
        var deps = await ObterIdsDependenciasAsync();
        var os = await CriarOrdemServicoAuxiliarAsync();

        await Client.PatchAsync($"/api/ordens-servico/{os.Id}/iniciar-diagnostico", null);

        var dtoServico = new AdicionarServicoDto(deps.ServicoId, 150.00m);
        await Client.PostAsJsonAsync($"/api/ordens-servico/{os.Id}/servicos", dtoServico);

        await Client.PatchAsync($"/api/ordens-servico/{os.Id}/enviar-para-aprovacao", null);

        await Client.PatchAsync($"/api/ordens-servico/{os.Id}/aprovar", null);

        var osDetalhe = await Client.GetFromJsonAsync<OrdemServicoDetalheDto>($"/api/ordens-servico/{os.Id}");
        var osServicoId = osDetalhe!.Servicos.First().Id;

        return (os.Id, osServicoId);
    }


    [Fact(DisplayName = "POST /api/ordens-servico - Criar OS válida deve retornar 201 Created")]
    public async Task CreateOrdemServico_ComDadosValidos_DeveRetornarCreated()
    {
        await AuthenticateAsync();
        var deps = await ObterIdsDependenciasAsync();

        var requestDto = new CriarOrdemServicoDto(deps.ClienteId, deps.VeiculoId, "Barulho na suspensão");

        var response = await Client.PostAsJsonAsync("/api/ordens-servico", requestDto);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Created, because: erro);
        var responseData = await response.Content.ReadFromJsonAsync<OrdemServicoDto>();
        responseData.Should().NotBeNull();
        responseData!.Id.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "GET /api/ordens-servico/{id} - OS existente deve retornar 200 OK com detalhes")]
    public async Task GetOrdemServicoById_QuandoExiste_DeveRetornarOk()
    {
        await AuthenticateAsync();
        var osCriada = await CriarOrdemServicoAuxiliarAsync();

        var response = await Client.GetAsync($"/api/ordens-servico/{osCriada.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<OrdemServicoDetalheDto>();
        responseData.Should().NotBeNull();
        responseData!.Id.Should().Be(osCriada.Id);
    }

    [Fact(DisplayName = "GET /api/ordens-servico - Deve retornar lista paginada e 200 OK")]
    public async Task GetAllOrdensServico_DeveRetornarOkEListaPaginada()
    {
        await AuthenticateAsync();
        await CriarOrdemServicoAuxiliarAsync();

        var response = await Client.GetAsync("/api/ordens-servico?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<PagedResult<OrdemServicoDto>>();
        responseData.Should().NotBeNull();
        responseData!.Items.Should().NotBeEmpty();
    }


    [Fact(DisplayName = "POST /api/ordens-servico/{id}/servicos - Deve adicionar serviço se OS Recebida/Diagnóstico")]
    public async Task AdicionarServico_QuandoOSStatusValido_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var deps = await ObterIdsDependenciasAsync();
        var osCriada = await CriarOrdemServicoAuxiliarAsync();

        var dto = new AdicionarServicoDto(deps.ServicoId, 200.50m);
        var response = await Client.PostAsJsonAsync($"/api/ordens-servico/{osCriada.Id}/servicos", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "POST /api/ordens-servico/{id}/insumos - Deve adicionar insumo externo com sucesso")]
    public async Task AdicionarInsumo_QuandoOSStatusValido_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var osCriada = await CriarOrdemServicoAuxiliarAsync();

        var dto = new AdicionarInsumoDto(null, "Óleo de Motor 5W40", 50.00m, 4, EOrigemInsumo.CompraEspecifica);
        var response = await Client.PostAsJsonAsync($"/api/ordens-servico/{osCriada.Id}/insumos", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "PATCH /api/ordens-servico/{id}/iniciar-diagnostico - Deve alterar status")]
    public async Task IniciarDiagnostico_QuandoStatusRecebida_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var osCriada = await CriarOrdemServicoAuxiliarAsync();

        var response = await Client.PatchAsync($"/api/ordens-servico/{osCriada.Id}/iniciar-diagnostico", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "PATCH /api/ordens-servico/{id}/enviar-para-aprovacao - Deve requerer serviço adicionado")]
    public async Task AguardarAprovacao_AposDiagnosticoEServico_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var deps = await ObterIdsDependenciasAsync();
        var os = await CriarOrdemServicoAuxiliarAsync();

        await Client.PatchAsync($"/api/ordens-servico/{os.Id}/iniciar-diagnostico", null);
        await Client.PostAsJsonAsync($"/api/ordens-servico/{os.Id}/servicos", new AdicionarServicoDto(deps.ServicoId, 100m));

        var response = await Client.PatchAsync($"/api/ordens-servico/{os.Id}/enviar-para-aprovacao", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "PATCH /api/ordens-servico/{id}/servicos/{servicoId}/iniciar - Deve iniciar serviço")]
    public async Task IniciarServico_QuandoOsEmExecucao_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var (osId, osServicoId) = await PrepararOsEmExecucaoAsync();

        var response = await Client.PatchAsync($"/api/ordens-servico/{osId}/servicos/{osServicoId}/iniciar", null);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, because: erro);
    }

    [Fact(DisplayName = "PATCH /api/ordens-servico/{id}/servicos/{servicoId}/concluir - Deve concluir serviço")]
    public async Task ConcluirServico_AposIniciado_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var (osId, osServicoId) = await PrepararOsEmExecucaoAsync();

        await Client.PatchAsync($"/api/ordens-servico/{osId}/servicos/{osServicoId}/iniciar", null);

        var response = await Client.PatchAsync($"/api/ordens-servico/{osId}/servicos/{osServicoId}/concluir", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "PATCH /api/ordens-servico/{id}/entregar - Deve falhar se não finalizada")]
    public async Task Entregar_SeNaoFinalizada_DeveRetornarBadRequest()
    {
        await AuthenticateAsync();
        var os = await CriarOrdemServicoAuxiliarAsync();

        var response = await Client.PatchAsync($"/api/ordens-servico/{os.Id}/entregar", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}