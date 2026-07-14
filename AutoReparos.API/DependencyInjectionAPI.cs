using AutoReparos.API.Controllers;
using AutoReparos.API.Handlers;
using AutoReparos.API.Services;
using AutoReparos.Application.Shared.Interfaces;
using AutoReparos.Infra.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AutoReparos.API
{
    public static class DependencyInjectionApi
    {
        /// <summary>
        /// Método de extensão para registrar serviços relacionados à camada de API
        /// </summary>
        /// <param name="services">Collection de serviços da aplicação</param>
        /// <param name="configuration">Configurações da aplicação</param>
        /// <returns>Collection de services com os serviços da API registrados</returns>
        public static IServiceCollection AddAPI(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.Configure<SeedUsuarioSettings>(configuration.GetSection(SeedUsuarioSettings.SectionName));

            var jwtSettings = new JwtSettings();
            configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

            if (string.IsNullOrEmpty(jwtSettings.Secret))
                throw new InvalidOperationException($"Configurações de JWT não encontradas ou inválidas na seção '{JwtSettings.SectionName}'. Verifique o arquivo de configuração ou as variáveis de ambiente.");

            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

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

            services.AddScoped<AuthController>();
            services.AddScoped<UsuarioController>();
            services.AddScoped<ClienteController>();
            services.AddScoped<VeiculoController>();
            services.AddScoped<ServicoController>();
            services.AddScoped<InsumoController>();
            services.AddScoped<OrdemServicoController>();
            services.AddScoped<OrdemServicoFluxoController>();
            services.AddScoped<OrdemServicoAprovacaoController>();

            services.AddHttpContextAccessor();
            services.AddScoped<IAppUrlProvider, HttpAppUrlProvider>();

            return services;
        }
    }
}
