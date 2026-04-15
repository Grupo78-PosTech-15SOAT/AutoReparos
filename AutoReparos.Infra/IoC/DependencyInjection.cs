using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Infra.Data;
using AutoReparos.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoReparos.Infra.IoC
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Método de extensão para registrar serviços relacionados à camada de Infraestructure
        /// </summary>
        /// <param name="services">Collection de serviços da aplicação</param>
        /// <returns>Collection de services com os serviços de Infraestructure registrados</returns>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DbConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            services.AddScoped<IClienteRepository, ClienteRepository>();

            services.AddScoped<IVeiculoRepository, VeiculoRepository>();

            return services;
        }
    }
}
