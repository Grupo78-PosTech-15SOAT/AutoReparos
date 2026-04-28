using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Pecas.Repositories;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Infra.Data;
using AutoReparos.Infra.Identity;
using AutoReparos.Infra.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoReparos.Infra.IoC
{
    public static class DependencyInjectionInfra
    {
        /// <summary>
        /// Método de extensão para registrar serviços relacionados à camada de Infraestructure
        /// </summary>
        /// <param name="services">Collection de serviços da aplicação</param>
        /// <returns>Collection de services com os serviços de Infraestructure registrados</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DbConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            services.AddIdentityCore<Usuario>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddErrorDescriber<IdentityErrosTranslation>()
                .AddDefaultTokenProviders();

            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IVeiculoRepository, VeiculoRepository>();
            services.AddScoped<IServicoRepository, ServicoRepository>();
            services.AddScoped<IPecaRepository, PecaRepository>();
            services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();

            return services;
        }
    }
}
