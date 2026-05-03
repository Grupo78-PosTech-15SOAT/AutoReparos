using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.Veiculos.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoReparos.Infra.Data.Mappings
{
    public class OrdemServicoMapping : IEntityTypeConfiguration<OrdemServico>
    {
        public void Configure(EntityTypeBuilder<OrdemServico> builder)
        {
            builder.HasKey(os => os.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(os => os.ClienteId)
                .IsRequired();

            builder.Property(os => os.VeiculoId)
                .IsRequired();

            builder.Property(os => os.Status)
                .IsRequired();

            builder.Property(os => os.Observacao)
                .HasMaxLength(500);

            builder.Property(os => os.CriadoEm)
                .IsRequired();

            builder.Property(os => os.IniciadoEm);
            builder.Property(os => os.FinalizadoEm);
            builder.Property(os => os.EntregueEm);

            builder.HasOne<Cliente>()
                .WithMany()
                .HasForeignKey(os => os.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Veiculo>()
                .WithMany()
                .HasForeignKey(os => os.VeiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(os => os.Servicos)
                .HasField("_servicos")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(os => os.Servicos)
                .WithOne()
                .HasForeignKey(os => os.OrdemServicoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(os => os.Insumos)
                .HasField("_insumos")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(os => os.Insumos)
                .WithOne()
                .HasForeignKey(os => os.OrdemServicoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
