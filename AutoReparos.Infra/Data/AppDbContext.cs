using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.Pecas.Entities;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Veiculos.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoReparos.Infra.Data
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Peca> Pecas { get; set; }
        public DbSet<OrdemServico> OrdensServico { get; set; }
        public DbSet<OrdemServicoServico> OrdensServicoServicos { get; set; }
        public DbSet<OrdemServicoPeca> OrdensServicoPecas { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
