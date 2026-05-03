using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Infra.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoReparos.Infra.Data
{
    public class AppDbContext(DbContextOptions options) : IdentityDbContext<UsuarioIdentity, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Insumo> Insumos { get; set; }
        public DbSet<OrdemServico> OrdensServico { get; set; }
        public DbSet<OrdemServicoServico> OrdensServicoServicos { get; set; }
        public DbSet<OrdemServicoInsumo> OrdensServicoInsumos { get; set; }
        public DbSet<UsuarioIdentity> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
