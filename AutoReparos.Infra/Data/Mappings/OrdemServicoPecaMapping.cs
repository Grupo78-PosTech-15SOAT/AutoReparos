using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.Pecas.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Infra.Data.Mappings
{
    public class OrdemServicoPecaMapping : IEntityTypeConfiguration<OrdemServicoPeca>
    {
        public void Configure(EntityTypeBuilder<OrdemServicoPeca> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(p => p.OrdemServicoId)
                .IsRequired();

            builder.Property(p => p.PecaId);

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

            builder.HasOne<Peca>()
                .WithMany()
                .HasForeignKey(p => p.PecaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
