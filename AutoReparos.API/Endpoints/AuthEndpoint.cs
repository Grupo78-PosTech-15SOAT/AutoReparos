using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;
using AutoReparos.Application.Auth.Services.Interfaces;

namespace AutoReparos.API.Endpoints
{
    public static class AuthEndpoint
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/auth")
                .WithTags("Autenticação");

            group.MapPost("/login", async (LoginRequestDTO loginRequest, IAuthService authService) =>
            {
                var result = await authService.Login(loginRequest);

                if (result == null)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(result);
            })
            .WithName("Login")
            .WithSummary("Realiza o login do usuário")
            .WithDescription("Valida as credenciais e retorna o token JWT")
            .Produces<LoginResponseDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
