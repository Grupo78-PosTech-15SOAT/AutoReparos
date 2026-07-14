using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Domain.Servicos.Entities;

namespace AutoReparos.Application.Servicos.Mappers
{
    public static class ServicoMapper
    {
        public static ServicoDto ToDto(Servico servico) => new(
            servico.Id, servico.Nome, servico.Descricao, servico.ValorTabelado, servico.CriadoEm, servico.AtualizadoEm
        );

        public static TempoMedioServicoDto ToTempoMedioDto((Guid ServicoId, string NomeServico, TimeSpan TempoMedio, int TotalExecucoes) tempoMedio) => new(
            tempoMedio.ServicoId, tempoMedio.NomeServico, tempoMedio.TempoMedio, tempoMedio.TotalExecucoes
        );
    }
}
