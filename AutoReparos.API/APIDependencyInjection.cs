using AutoReparos.API.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AutoReparos.API
{
    public static class APIDependencyInjection
    {
        /// <summary>
        /// Método de extensão para registrar serviços relacionados à camada de API
        /// </summary>
        /// <param name="services">Collection de serviços da aplicação</param>
        /// <param name="configuration">Configurações da aplicação</param>
        /// <returns>Collection de services com os serviços da API registrados</returns>
        public static IServiceCollection AddAPI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenApi("v1", o =>
            {
                o.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info = new()
                    {
                        Title = "AutoReparos API",
                        Description = "API criada para gestão de ordens de serviços de uma oficina de mecânica.",
                        Version = "v1"
                    };
                    return Task.CompletedTask;
                });
            });

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            var jwtSecret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret não encontrado.");
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAuthorization();

            return services;
        }
    }
}
