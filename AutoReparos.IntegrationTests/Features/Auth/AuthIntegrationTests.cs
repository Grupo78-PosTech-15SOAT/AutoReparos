using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Usuarios.Repositories;
using AutoReparos.IntegrationTests.Infrastructure;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AutoReparos.IntegrationTests.Features.Auth;

public class AuthIntegrationTests(CustomWebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
    [Fact(DisplayName = "Login com e-mail inexistente deve retornar 401 Unauthorized")]
    public async Task Login_ComEmailInexistente_DeveRetornarUnauthorized()
    {
        var faker = new Faker("pt_BR");
        var loginRequest = new LoginRequestDto(faker.Internet.Email(), faker.Internet.Password());

        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Login com senha errada para usuário existente deve retornar 401 Unauthorized")]
    public async Task Login_ComSenhaErrada_DeveRetornarUnauthorized()
    {
        // Arrange
        var faker = new Faker("pt_BR");
        var email = faker.Internet.Email().ToLowerInvariant();
        var correctPassword = "CorrectPassword@123";
        var wrongPassword = "WrongPassword@123";

        using (var scope = Services.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var usuario = new Usuario(faker.Person.FullName, email, ETipoUsuario.Mecanico);
            await repository.CreateAsync(usuario, correctPassword);
        }

        var loginRequest = new LoginRequestDto(email, wrongPassword);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Login com credenciais válidas deve retornar 200 OK e o Token")]
    public async Task Login_ComCredenciaisValidas_DeveRetornarOkEToken()
    {
        var loginRequest = new LoginRequestDto("admin@autoreparos.com", "Admin@123");

        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseData = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        responseData.Should().NotBeNull();
        responseData.Token.Should().NotBeNullOrWhiteSpace();
        responseData.Email.Should().Be("admin@autoreparos.com");
    }

    [Theory(DisplayName = "Login com credenciais válidas deve retornar a role correta correspondente ao perfil do usuário")]
    [InlineData(ETipoUsuario.Administrador, "Administrador")]
    [InlineData(ETipoUsuario.Mecanico, "Mecanico")]
    [InlineData(ETipoUsuario.Atendente, "Atendente")]
    public async Task Login_ComCredenciaisValidas_DeveRetornarRoleCorrespondente(ETipoUsuario tipoUsuario, string expectedRole)
    {
        // Arrange
        var faker = new Faker("pt_BR");
        var email = faker.Internet.Email().ToLowerInvariant();
        var password = "SenhaForte@123";
        var nome = faker.Person.FullName;

        using (var scope = Services.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var usuario = new Usuario(nome, email, tipoUsuario);
            await repository.CreateAsync(usuario, password);
        }

        var loginRequest = new LoginRequestDto(email, password);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseData = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        responseData.Should().NotBeNull();
        responseData!.Token.Should().NotBeNullOrWhiteSpace();
        responseData.Email.Should().Be(email);
        responseData.NomeCompleto.Should().Be(nome);
        responseData.Role.Should().Be(expectedRole);
    }
}