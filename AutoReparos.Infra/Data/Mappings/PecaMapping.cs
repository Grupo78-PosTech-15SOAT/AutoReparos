using AutoReparos.Domain.Pecas.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Infra.Data.Mappings
{
    public class PecaMapping : IEntityTypeConfiguration<Peca>
    {
        public void Configure(EntityTypeBuilder<Peca> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Descricao)
                .HasMaxLength(500);

            builder.Property(p => p.Valor)
                .IsRequired()
                .HasColumnType("numeric(18,2)");

            builder.Property(p => p.QuantidadeEstoque)
                .IsRequired();

            builder.Property(p => p.CriadoEm)
                .IsRequired();

            builder.Property(p => p.AtualizadoEm);
        }
    }
}
