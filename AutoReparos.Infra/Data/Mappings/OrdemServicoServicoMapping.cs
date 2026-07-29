using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.Servicos.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoReparos.Infra.Data.Mappings
{
    public class OrdemServicoServicoMapping : IEntityTypeConfiguration<OrdemServicoServico>
    {
        public void Configure(EntityTypeBuilder<OrdemServicoServico> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(s => s.OrdemServicoId)
                .IsRequired();

            builder.Property(s => s.ServicoId)
                .IsRequired();

            builder.Property(s => s.ValorCobrado)
                .IsRequired()
                .HasColumnType("numeric(18,2)");

            builder.Property(s => s.Status)
                .IsRequired();

            builder.Property(s => s.IniciadoEm);
            builder.Property(s => s.ConcluidoEm);

            builder.Ignore(s => s.TempoExecucao);

            builder.HasOne(s => s.Servico)
                .WithMany()
                .HasForeignKey(s => s.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
