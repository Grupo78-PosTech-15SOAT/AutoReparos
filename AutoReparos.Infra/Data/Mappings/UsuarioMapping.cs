using AutoReparos.Domain.Usuarios.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoReparos.Infra.Data.Mappings
{
    public class UsuarioMapping : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.Property(u => u.NomeCompleto)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(u => u.Tipo)
                .IsRequired();

            builder.Property(u => u.CriadoEm)
                .IsRequired();

            builder.Property(u => u.AtualizadoEm);

            builder.Property(u => u.Email)
                .HasMaxLength(256);

            builder.Property(u => u.UserName)
                .HasMaxLength(256);

            builder.Property(u => u.PasswordHash)
                .IsRequired();
        }
    }
}
