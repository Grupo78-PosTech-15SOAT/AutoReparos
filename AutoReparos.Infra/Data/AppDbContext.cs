using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Veiculos.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoReparos.Infra.Data
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<Veiculo> Veiculos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
