using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Usuarios.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoReparos.Infra.Data
{
    public static class DbInitializer
    {
        /// <summary>
        /// Método de extensão para orquestrar o Seed do banco de dados
        /// </summary>
        public static async Task SeedDatabase(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var userRepository = services.GetRequiredService<IUsuarioRepository>();
                var configuration = services.GetRequiredService<IConfiguration>();
                
                await SeedUsuariosAsync(userRepository, configuration);
            }
            catch (Exception ex)
            {
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DbInitializer");
                logger.LogError(ex, "Ocorreu um erro ao popular o banco de dados.");
            }
        }

        private static async Task SeedUsuariosAsync(IUsuarioRepository userRepository, IConfiguration configuration)
        {
            var adminEmailStr = configuration["SeedUser:Email"];
            var adminPassword = configuration["SeedUser:Password"];

            if (string.IsNullOrEmpty(adminEmailStr) || string.IsNullOrEmpty(adminPassword))
                return;

            var existingAdmin = await userRepository.GetByEmailAsync(adminEmailStr);

            if (existingAdmin == null)
            {
                var adminUser = new Usuario(
                    "Administrador do Sistema",
                    adminEmailStr,
                    ETipoUsuario.Administrador
                );

                await userRepository.CreateAsync(adminUser, adminPassword);
            }
        }
    }
}
