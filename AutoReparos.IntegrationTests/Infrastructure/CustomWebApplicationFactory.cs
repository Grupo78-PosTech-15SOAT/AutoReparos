using AutoReparos.IntegrationTests.Infrastructure.Config;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;
using Xunit;

namespace AutoReparos.IntegrationTests.Infrastructure
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
    {
        private const string ConnectionStringsDbConnection = "ConnectionStrings__DbConnection";
        private static readonly DbContainerSettings Settings = new();

        static CustomWebApplicationFactory()
        {
            var testConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json");
            if (File.Exists(testConfigPath))
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile(testConfigPath)
                    .Build();

                foreach (var kvp in config.AsEnumerable())
                {
                    if (!string.IsNullOrEmpty(kvp.Value) && string.IsNullOrEmpty(Environment.GetEnvironmentVariable(kvp.Key.Replace(":", "__"))))
                    {
                        Environment.SetEnvironmentVariable(kvp.Key.Replace(":", "__"), kvp.Value);
                    }
                }
            }
        }

        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder(Settings.Image)
            .WithDatabase(Settings.Database)
            .WithUsername(Settings.Username)
            .WithPassword(Settings.Password)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("database system is ready to accept connections"))
            .WithCleanUp(true)
            .Build();

        private Respawner? _respawner;
        private DbConnection? _dbConnection;

        public string ConnectionString => _dbContainer.GetConnectionString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                var testConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json");
                configBuilder.AddJsonFile(testConfigPath, optional: true);
            });
        }

        public async Task ResetDatabaseAsync()
        {
            if (_respawner == null)
            {
                _dbConnection = new NpgsqlConnection(ConnectionString);
                await _dbConnection.OpenAsync();

                _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
                {
                    DbAdapter = DbAdapter.Postgres,
                    SchemasToInclude = ["public"],
                    TablesToIgnore = ["__EFMigrationsHistory"],
                    WithReseed = true
                });
            }

            if (_dbConnection != null)
            {
                await _respawner.ResetAsync(_dbConnection);
            }
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            Environment.SetEnvironmentVariable(ConnectionStringsDbConnection, ConnectionString);
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            Environment.SetEnvironmentVariable(ConnectionStringsDbConnection, null);
            if (_dbConnection != null)
            {
                await _dbConnection.DisposeAsync();
            }
            await _dbContainer.StopAsync();
        }
    }
}
