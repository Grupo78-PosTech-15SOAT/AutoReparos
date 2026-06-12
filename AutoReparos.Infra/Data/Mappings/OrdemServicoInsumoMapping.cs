using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.OrdensServicos.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoReparos.Infra.Data.Mappings
{
    public class OrdemServicoInsumoMapping : IEntityTypeConfiguration<OrdemServicoInsumo>
    {
        public void Configure(EntityTypeBuilder<OrdemServicoInsumo> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(p => p.OrdemServicoId)
                .IsRequired();

            builder.Property(p => p.InsumoId);

            builder.Property(p => p.Descricao)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.ValorUnitario)
                .IsRequired()
                .HasColumnType("numeric(18,2)");

            builder.Property(p => p.Quantidade)
                .IsRequired();

            builder.Property(p => p.Origem)
                .IsRequired();

            builder.Ignore(p => p.ValorTotal);

            builder.HasOne<Insumo>()
                .WithMany()
                .HasForeignKey(p => p.InsumoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
