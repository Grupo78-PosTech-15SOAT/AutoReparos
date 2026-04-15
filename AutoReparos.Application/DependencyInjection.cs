using AutoReparos.Application.Clientes.Services;
using AutoReparos.Application.Clientes.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AutoReparos.Application
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Método de extensão para registrar serviços relacionados à camada de Application
        /// </summary>
        /// <param name="services">Collection de serviços da aplicação</param>
        /// <returns>Collection de services com os serviços de Application registrados</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IClienteService, ClienteService>();

            return services;
        }
    }
}
