using AutoReparos.Infra.Settings;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Usuarios.Repositories;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoReparos.Infra.Data
{
    public static class DbInitializer
    {
        /// <summary>
        /// Método de extensão para orquestrar o Seed do banco de dados
        /// </summary>
        /// <param name="serviceProvider">ServiceProvider da aplicação</param>
        /// <param name="skipMigration">Se verdadeiro, pula a execução das migrações (útil em testes onde a migração já ocorreu)</param>
        public static async Task SeedDataAsync(IServiceProvider serviceProvider, bool skipMigration = false)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DbInitializer");

                if (!skipMigration)
                {
                    logger.LogInformation("Iniciando migração do banco de dados...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migração concluída.");
                }

                logger.LogInformation("Iniciando Seed...");

                var userRepository = services.GetRequiredService<IUsuarioRepository>();
                var clienteRepository = services.GetRequiredService<IClienteRepository>();
                var seedSettings = services.GetRequiredService<IOptions<SeedUsuarioSettings>>().Value;
                var environment = services.GetRequiredService<IHostEnvironment>();

                await SeedUsuariosAsync(userRepository, seedSettings, logger);

                if (environment.IsDevelopment() || environment.IsEnvironment("Testing") || environment.IsStaging())
                {
                    await SeedClientesAsync(clienteRepository, logger);
                }
            }
            catch (Exception ex)
            {
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DbInitializer");
                logger.LogError(ex, "Ocorreu um erro ao inicializar ou popular o banco de dados.");
                throw new InvalidOperationException("Erro durante a inicialização e seed do banco de dados.", ex);
            }
        }
        private static async Task SeedUsuariosAsync(IUsuarioRepository userRepository, SeedUsuarioSettings settings, ILogger logger)
        {
            if (string.IsNullOrEmpty(settings.Email) || string.IsNullOrEmpty(settings.Password))
            {
                logger.LogWarning("Configurações de Seed de usuário não encontradas ou incompletas.");
                return;
            }

            var existingAdmin = await userRepository.GetByEmailAsync(settings.Email);

            if (existingAdmin == null)
            {
                logger.LogInformation("Criando usuário administrador padrão: {Email}", settings.Email);
                var adminUser = new Usuario(
                    "Administrador do Sistema",
                    settings.Email,
                    ETipoUsuario.Administrador
                );

                await userRepository.CreateAsync(adminUser, settings.Password);
                logger.LogInformation("Usuário administrador criado com sucesso.");
            }
        }

        private static async Task SeedClientesAsync(IClienteRepository clienteRepository, ILogger logger)
        {
            var (_, total) = await clienteRepository.GetAll(null, 0, 1);

            if (total == 0)
            {
                logger.LogInformation("Criando cliente padrão para ambiente de desenvolvimento/testes.");
                var clientePadrao = new Cliente(
                    "Cliente Teste Padrão",
                    "98765432100",
                    "11999999999",
                    "cliente@teste.com"
                );
                await clienteRepository.Create(clientePadrao);
                logger.LogInformation("Cliente padrão criado com sucesso.");
            }
        }
    }
}
