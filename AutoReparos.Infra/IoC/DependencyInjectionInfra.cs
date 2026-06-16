using AutoReparos.Application.Shared.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Shared.Interfaces;
using AutoReparos.Domain.Usuarios.Repositories;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Infra.Data;
using AutoReparos.Infra.Identity;
using AutoReparos.Infra.Identity.Models;
using AutoReparos.Infra.Identity.Services;
using AutoReparos.Infra.Repositories;
using AutoReparos.Infra.Services;
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
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                options.UseNpgsql(config.GetConnectionString("DbConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            });

            services.AddIdentityCore<UsuarioIdentity>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddErrorDescriber<IdentityErrosTranslation>()
                .AddDefaultTokenProviders();

            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IVeiculoRepository, VeiculoRepository>();
            services.AddScoped<IServicoRepository, ServicoRepository>();
            services.AddScoped<IInsumoRepository, InsumoRepository>();
            services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAprovacaoTokenService, AprovacaoTokenService>();

            return services;
        }
    }
}
