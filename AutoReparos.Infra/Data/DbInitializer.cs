using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Clientes.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AutoReparos.Infra.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(UserManager<Usuario> userManager, IConfiguration configuration)
        {
            var adminEmailStr = configuration["SeedUser:Email"];
            var adminPassword = configuration["SeedUser:Password"];

            if (string.IsNullOrEmpty(adminEmailStr) || string.IsNullOrEmpty(adminPassword))
                return;

            if (!await userManager.Users.AnyAsync())
            {
                var adminEmail = Email.Create(adminEmailStr);
                var adminUser = new Usuario(
                    "Administrador do Sistema",
                    adminEmail,
                    ETipoUsuario.Administrador
                );

                await userManager.CreateAsync(adminUser, adminPassword);
            }
        }
    }
}
