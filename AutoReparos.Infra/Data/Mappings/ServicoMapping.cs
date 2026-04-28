using AutoReparos.Domain.Servicos.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoReparos.Infra.Data.Mappings
{
    public class ServicoMapping : IEntityTypeConfiguration<Servico>
    {
        public void Configure(EntityTypeBuilder<Servico> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Descricao)
                .HasMaxLength(500);

            builder.Property(s => s.ValorTabelado)
                .HasColumnType("numeric(18,2)");

            builder.Property(s => s.CriadoEm)
                .IsRequired();

            builder.Property(s => s.AtualizadoEm);
        }
    }
}
