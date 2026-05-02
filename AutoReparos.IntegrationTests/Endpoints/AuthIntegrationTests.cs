using System.Net;
using System.Net.Http.Json;
using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;
using Bogus;
using FluentAssertions;
using Xunit;

namespace AutoReparos.IntegrationTests.Endpoints;

[Collection("Integration Tests")]
public class AuthIntegrationTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact(DisplayName = "Login com credenciais inválidas deve retornar 401 Unauthorized")]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        var faker = new Faker("pt_BR");
        var loginRequest = new LoginRequestDTO(faker.Internet.Email(), faker.Internet.Password());

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Login com credenciais válidas deve retornar 200 OK e o Token")]
    public async Task Login_ComCredenciaisValidas_DeveRetornarOkEToken()
    {
        var loginRequest = new LoginRequestDTO("admin@autoreparos.com", "Admin@123");

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseData = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
        responseData.Should().NotBeNull();
        responseData!.Token.Should().NotBeNullOrWhiteSpace();
    }
}