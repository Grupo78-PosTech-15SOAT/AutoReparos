using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Veiculos.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoReparos.Infra.Data.Mappings
{
    public class VeiculoMapping : IEntityTypeConfiguration<Veiculo>
    {
        public void Configure(EntityTypeBuilder<Veiculo> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.ClienteId)
                .IsRequired();

            builder.Property(v => v.Marca)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(v => v.Modelo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(v => v.AnoFabricacao)
                .IsRequired();

            builder.Property(v => v.AnoModelo)
                .IsRequired();

            builder.Property(v => v.CriadoEm)
                .IsRequired();

            builder.Property(v => v.AtualizadoEm);

            builder.HasOne<Cliente>()
                .WithMany()
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(v => v.Placa, placa =>
            {
                placa.Property(p => p.Valor)
                    .HasColumnName("Placa")
                    .IsRequired()
                    .HasMaxLength(7);

                placa.HasIndex(p => p.Valor)
                    .IsUnique()
                    .HasDatabaseName("IX_Veiculos_Placa");
            });

            builder.OwnsOne(v => v.Chassi, chassi =>
            {
                chassi.Property(c => c.Valor)
                    .HasColumnName("Chassi")
                    .IsRequired()
                    .HasMaxLength(17);

                chassi.HasIndex(c => c.Valor)
                    .IsUnique()
                    .HasDatabaseName("IX_Veiculos_Chassi");
            });

            builder.OwnsOne(v => v.Renavam, renavam =>
            {
                renavam.Property(r => r.Valor)
                    .HasColumnName("Renavam")
                    .IsRequired()
                    .HasMaxLength(11);

                renavam.HasIndex(r => r.Valor)
                    .IsUnique()
                    .HasDatabaseName("IX_Veiculos_Renavam");
            });
        }
    }
}