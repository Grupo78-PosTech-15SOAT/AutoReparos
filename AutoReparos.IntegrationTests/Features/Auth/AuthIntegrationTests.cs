using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;
using AutoReparos.IntegrationTests.Infrastructure;
using Bogus;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AutoReparos.IntegrationTests.Features.Auth;

public class AuthIntegrationTests(CustomWebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
    [Fact(DisplayName = "Login com credenciais inválidas deve retornar 401 Unauthorized")]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        var faker = new Faker("pt_BR");
        var loginRequest = new LoginRequestDto(faker.Internet.Email(), faker.Internet.Password());

        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

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
}