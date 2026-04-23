using AutoReparos.Application.Clientes.Services;
using AutoReparos.Application.Clientes.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.Services;
using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.Pecas.Services;
using AutoReparos.Application.Pecas.Services.Interfaces;
using AutoReparos.Application.Servicos.Services;
using AutoReparos.Application.Servicos.Services.Interfaces;
using AutoReparos.Application.Usuarios.Services;
using AutoReparos.Application.Usuarios.Services.Interfaces;
using AutoReparos.Application.Veiculos.Services;
using AutoReparos.Application.Veiculos.Services.Interfaces;
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
            services.AddScoped<IVeiculoService, VeiculoService>();
            services.AddScoped<IServicoService, ServicoService>();
            services.AddScoped<IPecaService, PecaService>();
            services.AddScoped<IOrdemServicoService, OrdemServicoService>();
            services.AddScoped<IUsuarioService, UsuarioService>();

            return services;
        }
    }
}
