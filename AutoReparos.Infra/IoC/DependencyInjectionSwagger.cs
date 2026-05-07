using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace AutoReparos.Infra.IoC
{

    public static class DependencyInjectionSwagger
    {
        /// <summary>
        /// Método de extensão para registrar serviços relacionados ao Swagger, responsável por gerar a documentação da API e fornecer uma interface interativa para testar os endpoints.
        /// </summary>
        /// <param name="services">Collection de serviços da aplicação</param>
        /// <returns>Collection de services com o Swagger registrado</returns>
        public static IServiceCollection AddInfraestructureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "AutoReparos API", Description = "API criada para gestão de ordens de serviços de uma oficina de mecânica.", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Autenticação JWT",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira apenas o token JWT.\r\n\r\nNão é necessário informar o prefixo 'Bearer'."
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", document),
                        new List<string>()
                    }
                });
            });

            return services;
        }
    }
}
