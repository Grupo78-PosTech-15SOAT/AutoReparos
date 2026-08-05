using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Usuarios.Repositories;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;
using AutoReparos.Infra.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoReparos.Infra.Data
{
    public static class DbInitializer
    {
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

                var usuarioRepository = services.GetRequiredService<IUsuarioRepository>();
                var clienteRepository = services.GetRequiredService<IClienteRepository>();
                var veiculoRepository = services.GetRequiredService<IVeiculoRepository>();
                var insumoRepository = services.GetRequiredService<IInsumoRepository>();
                var servicoRepository = services.GetRequiredService<IServicoRepository>();
                var osRepository = services.GetRequiredService<IOrdemServicoRepository>();
                var seedSettings = services.GetRequiredService<IOptions<SeedUsuarioSettings>>().Value;
                var environment = services.GetRequiredService<IHostEnvironment>();

                await SeedUsuariosAsync(usuarioRepository, seedSettings, logger);

                if (environment.IsDevelopment() || environment.IsEnvironment("Testing") || environment.IsStaging())
                {
                    var clientes = await SeedClientesAsync(clienteRepository, logger);
                    var veiculos = await SeedVeiculosAsync(veiculoRepository, clientes, logger);
                    var insumos = await SeedInsumosAsync(insumoRepository, logger);
                    var servicos = await SeedServicosAsync(servicoRepository, logger);
                    await SeedOrdensServicoAsync(osRepository, clientes, veiculos, servicos, insumos, logger);
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

        private static async Task SeedUsuariosAsync(
            IUsuarioRepository usuarioRepository,
            SeedUsuarioSettings settings,
            ILogger logger)
        {
            if (string.IsNullOrEmpty(settings.Email) || string.IsNullOrEmpty(settings.Password))
            {
                logger.LogWarning("Configurações de Seed de usuário não encontradas ou incompletas.");
                return;
            }

            var existingAdmin = await usuarioRepository.GetByEmailAsync(settings.Email);
            if (existingAdmin is not null) return;

            logger.LogInformation("Criando usuário administrador padrão: {Email}", settings.Email);

            var adminUser = new Usuario("Administrador do Sistema", settings.Email, ETipoUsuario.Administrador);
            await usuarioRepository.CreateAsync(adminUser, settings.Password);

            logger.LogInformation("Usuário administrador criado com sucesso.");
        }

        private static async Task<List<Cliente>> SeedClientesAsync(
            IClienteRepository clienteRepository,
            ILogger logger)
        {
            var (_, total) = await clienteRepository.GetAll(null, 0, 1);
            if (total > 0)
            {
                logger.LogInformation("Clientes já existem, pulando seed.");
                var (existentes, _) = await clienteRepository.GetAll(null, 0, 3);
                return existentes.ToList();
            }

            logger.LogInformation("Criando clientes de desenvolvimento...");

            var clientes = new List<Cliente>
            {
                new("Leandro Tavares", "97632180044", "27999111111", "leandro.silva@email.com"),
                new("Fernanda Oliveira Costa", "48743358020", "27999222222", "fernanda.costa@email.com"),
                new("Roberto Alves Pereira", "60729292061", "27999333333", "roberto.pereira@email.com"),
            };

            foreach (var cliente in clientes)
                await clienteRepository.Create(cliente);

            logger.LogInformation("{Count} clientes criados.", clientes.Count);
            return clientes;
        }

        private static async Task<List<Veiculo>> SeedVeiculosAsync(
            IVeiculoRepository veiculoRepository,
            List<Cliente> clientes,
            ILogger logger)
        {
            var (_, total) = await veiculoRepository.GetAll(null, 0, 1);
            if (total > 0)
            {
                logger.LogInformation("Veículos já existem, pulando seed.");
                var (existentes, _) = await veiculoRepository.GetAll(null, 0, 3);
                return existentes.ToList();
            }

            logger.LogInformation("Criando veículos de desenvolvimento...");

            var veiculos = new List<Veiculo>
            {
                new(clientes[0].Id, "Volkswagen", "Gol", 2019, 2020,
                    new Placa("ABC1D23"),
                    new Chassi("9BWZZZ377VT004251"),
                    new Renavam("12345678901")),

                new(clientes[1].Id, "Chevrolet", "Onix", 2021, 2021,
                    new Placa("XYZ2E34"),
                    new Chassi("9BWZZZ377VT004252"),
                    new Renavam("23456789012")),

                new(clientes[2].Id, "Fiat", "Strada", 2020, 2021,
                    new Placa("DEF3F45"),
                    new Chassi("9BWZZZ377VT004253"),
                    new Renavam("34567890123")),
            };

            foreach (var veiculo in veiculos)
                await veiculoRepository.Create(veiculo);

            logger.LogInformation("{Count} veículos criados.", veiculos.Count);
            return veiculos;
        }

        private static async Task<List<Insumo>> SeedInsumosAsync(
            IInsumoRepository insumoRepository,
            ILogger logger)
        {
            var (_, total) = await insumoRepository.GetAll(null, 0, 1);
            if (total > 0)
            {
                logger.LogInformation("Insumos já existem, pulando seed.");
                var (existentes, _) = await insumoRepository.GetAll(null, 0, 3);
                return existentes.ToList();
            }

            logger.LogInformation("Criando insumos de desenvolvimento...");

            var insumos = new List<Insumo>
            {
                new("Óleo Motor 5W30", "Óleo sintético para motor", 45.90m, 20),
                new("Filtro de Óleo", "Filtro compatível com modelos 1.0/1.4", 28.50m, 15),
                new("Pastilha de Freio", "Pastilha dianteira para veículos populares", 89.90m, 10),
            };

            foreach (var insumo in insumos)
                await insumoRepository.Create(insumo);

            logger.LogInformation("{Count} insumos criados.", insumos.Count);
            return insumos;
        }

        private static async Task<List<Servico>> SeedServicosAsync(
            IServicoRepository servicoRepository,
            ILogger logger)
        {
            var (_, total) = await servicoRepository.GetAll(null, 0, 1);
            if (total > 0)
            {
                logger.LogInformation("Serviços já existem, pulando seed.");
                var (existentes, _) = await servicoRepository.GetAll(null, 0, 3);
                return existentes.ToList();
            }

            logger.LogInformation("Criando serviços de desenvolvimento...");

            var servicos = new List<Servico>
            {
                new("Troca de Óleo", "Troca de óleo e filtro", 120.00m),
                new("Revisão de Freios", "Inspeção e troca de pastilhas", 200.00m),
                new("Alinhamento", "Alinhamento e balanceamento", 150.00m),
            };

            foreach (var servico in servicos)
                await servicoRepository.Create(servico);

            logger.LogInformation("{Count} serviços criados.", servicos.Count);
            return servicos;
        }

        // OS1 com "EmExecucao", OS2 com "AguardandoAprovacao" e OS3 com "Recebida"
        private static async Task SeedOrdensServicoAsync(
            IOrdemServicoRepository osRepository,
            List<Cliente> clientes,
            List<Veiculo> veiculos,
            List<Servico> servicos,
            List<Insumo> insumos,
            ILogger logger)
        {
            var (_, total) = await osRepository.GetAll(null, null, null, 0, 1);
            if (total > 0)
            {
                logger.LogInformation("Ordens de Serviço já existem, pulando seed.");
                return;
            }

            logger.LogInformation("Criando Ordens de Serviço de desenvolvimento...");

            const string seedUser = "seed-admin";

            var os1 = new OrdemServico(clientes[0].Id, veiculos[0].Id, "Troca de óleo e revisão de freios");
            os1.AdicionarServico(new OrdemServicoServico(os1.Id, servicos[0].Id, servicos[0].ValorTabelado!.Value));
            os1.AdicionarInsumo(new OrdemServicoInsumo(os1.Id, insumos[0].Id, insumos[0].Nome, insumos[0].Valor, 1, EOrigemInsumo.Estoque));
            os1.IniciarDiagnostico(seedUser);
            os1.AguardarAprovacao(seedUser);
            os1.Aprovar();
            await osRepository.Create(os1);

            var os2 = new OrdemServico(clientes[1].Id, veiculos[1].Id, "Revisão completa de freios");
            os2.AdicionarServico(new OrdemServicoServico(os2.Id, servicos[1].Id, servicos[1].ValorTabelado!.Value));
            os2.AdicionarInsumo(new OrdemServicoInsumo(os2.Id, insumos[2].Id, insumos[2].Nome, insumos[2].Valor, 2, EOrigemInsumo.Estoque));
            os2.IniciarDiagnostico(seedUser);
            os2.AguardarAprovacao(seedUser);
            await osRepository.Create(os2);

            var os3 = new OrdemServico(clientes[2].Id, veiculos[2].Id, "Verificar alinhamento e balanceamento");
            await osRepository.Create(os3);

            logger.LogInformation("3 Ordens de Serviço criadas (EmExecucao, AguardandoAprovacao, Recebida).");
        }
    }
}