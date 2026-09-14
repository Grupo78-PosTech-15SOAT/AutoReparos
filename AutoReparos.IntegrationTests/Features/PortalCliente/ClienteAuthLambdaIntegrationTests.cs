using Amazon.Lambda.APIGatewayEvents;
using AutoReparos.AuthLambda.Functions;
using AutoReparos.AuthLambda.Models;
using AutoReparos.AuthLambda.Services;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using Xunit;

namespace AutoReparos.IntegrationTests.Features.PortalCliente;

[Collection("IntegrationTests")]
public class ClienteAuthLambdaIntegrationTests(CustomWebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
    private const string JwtSecret = "test_jwt_dummy_secret_key_with_32_chars_ok!";

    private ClienteAuthFunction CriarLambdaFunction()
    {
        var cpfService = new CpfValidationService();
        var dbService = new ClienteAuthDatabaseService(Factory.ConnectionString);
        var tokenService = new TokenGenerationService(JwtSecret, expiryHours: 1);

        return new ClienteAuthFunction(cpfService, dbService, tokenService);
    }

    [Fact(DisplayName = "Lambda Auth - Deve autenticar cliente ativo existente no PostgreSQL e retornar 200 OK com JWT")]
    public async Task LambdaAuth_ClienteAtivoExistente_DeveRetornar200ComToken()
    {
        // Arrange
        var lambda = CriarLambdaFunction();
        // Leandro Tavares seeded em DbInitializer: CPF 97632180044, email leandro.silva@email.com
        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new
            {
                cpf = "976.321.800-44",
                email = " Leandro.Silva@Email.com "
            })
        };

        // Act
        var response = await lambda.FunctionHandler(request);

        // Assert
        response.StatusCode.Should().Be(200);
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");

        var responseDto = JsonSerializer.Deserialize<ClienteAuthResponse>(response.Body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        responseDto.Should().NotBeNull();
        responseDto!.Token.Should().NotBeNullOrWhiteSpace();
        responseDto.Nome.Should().Be("Leandro Tavares");
        responseDto.Email.Should().Be("leandro.silva@email.com");
        responseDto.Role.Should().Be("Cliente");
        responseDto.ExpiresIn.Should().Be(3600);
    }

    [Fact(DisplayName = "Lambda Auth - Deve retornar 401 Unauthorized quando credenciais divergirem")]
    public async Task LambdaAuth_CredenciaisDivergentes_DeveRetornar401()
    {
        // Arrange
        var lambda = CriarLambdaFunction();
        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new
            {
                cpf = "97632180044",
                email = "email_errado@email.com"
            })
        };

        // Act
        var response = await lambda.FunctionHandler(request);

        // Assert
        response.StatusCode.Should().Be(401);
        response.Body.Should().Contain("não localizado ou dados divergentes");
    }

    [Fact(DisplayName = "Lambda Auth - Deve retornar 403 Forbidden quando cliente estiver inativo")]
    public async Task LambdaAuth_ClienteInativo_DeveRetornar403()
    {
        // Arrange: cria e inativa um cliente diretamente no PostgreSQL
        using var scope = Factory.Services.CreateScope();
        var clienteRepo = scope.ServiceProvider.GetRequiredService<IClienteRepository>();

        var clienteInativo = new Cliente("Cliente Inativado Teste", "52998224725", "27999888888", "inativo.teste@email.com");
        clienteInativo.Inativar();
        await clienteRepo.Create(clienteInativo);

        var lambda = CriarLambdaFunction();
        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "POST",
            Body = JsonSerializer.Serialize(new
            {
                cpf = "529.982.247-25",
                email = "inativo.teste@email.com"
            })
        };

        // Act
        var response = await lambda.FunctionHandler(request);

        // Assert
        response.StatusCode.Should().Be(403);
        response.Body.Should().Contain("encontra-se inativo");
    }
}
