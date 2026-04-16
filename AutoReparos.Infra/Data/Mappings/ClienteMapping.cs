using AutoReparos.Domain.Clientes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoReparos.Infra.Data.Mappings
{
    public class ClienteMapping : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Telefone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.CriadoEm)
                .IsRequired();

            builder.Property(e => e.AtualizadoEm);

            builder.OwnsOne(e => e.Email, email =>
            {
                email.Property(e => e.Address)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(200);

                email.HasIndex(e => e.Address)
                    .IsUnique()
                    .HasDatabaseName("IX_Clientes_Email");
            });

            builder.OwnsOne(e => e.Documento, doc =>
            {
                doc.Property(d => d.Valor)
                    .HasColumnName("Documento")
                    .IsRequired()
                    .HasMaxLength(14);

                doc.Property(d => d.Tipo)
                    .HasColumnName("TipoDocumento")
                    .IsRequired();

                doc.HasIndex(d => d.Valor)
                    .IsUnique()
                    .HasDatabaseName("IX_Clientes_Documento");
            });
        }
    }
}