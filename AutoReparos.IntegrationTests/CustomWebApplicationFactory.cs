using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Infra.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AutoReparos.IntegrationTests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string _dbName = $"AutoReparos_TestDB_{Guid.NewGuid()}";
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                {
                    var testConnectionString = $"Host=localhost;Port=5432;Database={_dbName};Username=admin;Password=1q2w3e4r@#$;Include Error Detail=true";

                    options.UseNpgsql(testConnectionString);
                });
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using (var scope = host.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var emailAdmin = "admin@autoreparos.com";
                if (!db.Usuarios.Any(u => u.Email == emailAdmin))
                {
                    var adminUser = new Usuario(
                        nomeCompleto: "Administrador Teste",
                        email: emailAdmin,
                        tipo: ETipoUsuario.Administrador
                    );

                    var hasher = new PasswordHasher<Usuario>();
                    adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

                    db.Usuarios.Add(adminUser);

                    if (!db.Clientes.Any())
                    {
                        var clientePadrao = new Cliente(
                            "Cliente Teste Padrão",
                            "98765432100", 
                            "11999999999",
                            "cliente@teste.com"
                        );
                        db.Clientes.Add(clientePadrao);
                    }



                    db.SaveChanges();
                }
            }

            return host;
        }
    }
}