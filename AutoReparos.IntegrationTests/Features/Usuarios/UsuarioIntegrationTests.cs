using AutoReparos.Application.Shared;
using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.DTOs.Response;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.IntegrationTests.Infrastructure;
using Bogus;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AutoReparos.IntegrationTests.Features.Usuarios;

public class UsuarioIntegrationTests(CustomWebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
    private async Task<UsuarioDto> CriarUsuarioAuxiliarAsync()
    {
        var faker = new Faker("pt_BR");

        var createDto = new UsuarioCreateDto
        (
            faker.Person.FullName,
            faker.Internet.Email(),
            "SenhaForte@123",
            ETipoUsuario.Administrador
        );

        var response = await Client.PostAsJsonAsync("/api/usuarios", createDto);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            throw new Exception($"Falha ao criar usuário auxiliar: {erro}");
        }

        return (await response.Content.ReadFromJsonAsync<UsuarioDto>())!;
    }

    [Fact(DisplayName = "POST /api/usuarios - Criar usuário válido deve retornar 201 Created")]
    public async Task CreateUsuario_ComDadosValidos_DeveRetornarCreated()
    {
        await AuthenticateAsync();
        var faker = new Faker("pt_BR");

        var requestDto = new UsuarioCreateDto(
            faker.Person.FullName,
            faker.Internet.Email(),
            "Senha@123",
            ETipoUsuario.Mecanico
        );

        var response = await Client.PostAsJsonAsync("/api/usuarios", requestDto);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Created, because: $"Erro retornado pela API: {erro}");

        var responseData = await response.Content.ReadFromJsonAsync<UsuarioDto>();
        responseData.Should().NotBeNull();
        responseData!.Id.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /api/usuarios - Deve retornar lista paginada e 200 OK")]
    public async Task GetAllUsuarios_DeveRetornarOkEListaPaginada()
    {
        await AuthenticateAsync();
        await CriarUsuarioAuxiliarAsync();

        var response = await Client.GetAsync("/api/usuarios?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<PagedResult<UsuarioDto>>();

        responseData.Should().NotBeNull();
        responseData!.Items.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "GET /api/usuarios/{id} - Usuário inexistente deve retornar 404 NotFound")]
    public async Task GetUsuarioById_QuandoNaoExiste_DeveRetornarNotFound()
    {
        await AuthenticateAsync();

        var response = await Client.GetAsync($"/api/usuarios/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET /api/usuarios/{id} - Quando usuário existe deve retornar 200 OK")]
    public async Task GetUsuarioById_QuandoExiste_DeveRetornarOk()
    {
        await AuthenticateAsync();
        var usuarioCriado = await CriarUsuarioAuxiliarAsync();

        var response = await Client.GetAsync($"/api/usuarios/{usuarioCriado.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseData = await response.Content.ReadFromJsonAsync<UsuarioDto>();
        responseData.Should().NotBeNull();
        responseData!.Id.Should().Be(usuarioCriado.Id);
    }

    [Fact(DisplayName = "PUT /api/usuarios/{id} - Atualizar usuário deve retornar 204 NoContent")]
    public async Task UpdateUsuario_ComDadosValidos_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var usuarioCriado = await CriarUsuarioAuxiliarAsync();

        var updateDto = new UsuarioUpdateDto("Nome Atualizado Pelo Teste", ETipoUsuario.Atendente);

        var response = await Client.PutAsJsonAsync($"/api/usuarios/{usuarioCriado.Id}", updateDto);
        var erro = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, because: erro);
    }

    [Fact(DisplayName = "DELETE /api/usuarios/{id} - Deletar usuário deve retornar 204 NoContent")]
    public async Task DeleteUsuario_QuandoExiste_DeveRetornarNoContent()
    {
        await AuthenticateAsync();
        var usuarioCriado = await CriarUsuarioAuxiliarAsync();

        var response = await Client.DeleteAsync($"/api/usuarios/{usuarioCriado.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Create Usuario Without Authentication Should Return Unauthorized")]
    public async Task CreateUsuario_WithoutAuth_ShouldReturnUnauthorized()
    {
        var createDto = new UsuarioCreateDto("Unauthorized", "unauth@test.com", "Pass123!", ETipoUsuario.Mecanico);
        var response = await Client.PostAsJsonAsync("/api/usuarios", createDto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}