using AutoReparos.Infra.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoReparos.Infra.Data.Mappings
{
    public class UsuarioMapping : IEntityTypeConfiguration<UsuarioIdentity>
    {
        public void Configure(EntityTypeBuilder<UsuarioIdentity> builder)
        {
            builder.Property(u => u.NomeCompleto)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(u => u.Tipo)
                .IsRequired();

            builder.Property(u => u.CriadoEm)
                .IsRequired();

            builder.Property(u => u.AtualizadoEm);

            builder.Property(u => u.PasswordHash)
                .IsRequired();
        }
    }
}
