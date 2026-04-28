using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
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
                var adminUser = new Usuario(
                    "Administrador do Sistema",
                    adminEmailStr,
                    ETipoUsuario.Administrador
                );

                await userManager.CreateAsync(adminUser, adminPassword);
            }
        }
    }
}
