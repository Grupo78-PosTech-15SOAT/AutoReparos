using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Domain.OrdensServicos.Entities;

namespace AutoReparos.Application.OrdensServicos.Mappers
{
    public static class OrdemServicoMapper
    {
        public static OrdemServicoDto ToDto(OrdemServico os) => new(
            os.Id, os.ClienteId, os.VeiculoId, os.Status.ToString(),
            os.Observacao, os.ValorTotal, os.CriadoEm,
            os.IniciadoEm, os.FinalizadoEm, os.EntregueEm
        );

        public static OrdemServicoPublicoDto ToPublicDto(OrdemServico os) => new(
            os.Id, os.Status.ToString(), os.Observacao, os.CriadoEm,
            os.IniciadoEm, os.FinalizadoEm, os.EntregueEm
        );

        public static OrdemServicoDetalheDto ToDetalheDto(OrdemServico os) => new(
            os.Id, os.ClienteId, os.VeiculoId, os.Status.ToString(),
            os.Observacao, os.ValorTotal, os.CriadoEm,
            os.IniciadoEm, os.FinalizadoEm, os.EntregueEm,
            os.Servicos.Select(s => new OrdemServicoServicoDto(
                s.Id, s.ServicoId, s.ValorCobrado, s.Status.ToString(),
                s.IniciadoEm, s.ConcluidoEm, s.TempoExecucao)),
            os.Insumos.Select(p => new OrdemServicoInsumoDto(
                p.Id, p.InsumoId, p.Descricao, p.ValorUnitario,
                p.Quantidade, p.ValorTotal, p.Origem.ToString()))
        );

        public static OrdemServicoPublicoDetalheDto ToPublicDetalheDto(OrdemServico os) => new(
            os.Id, os.Status.ToString(), os.Observacao, os.CriadoEm,
            os.IniciadoEm, os.FinalizadoEm, os.EntregueEm,
            os.Servicos.Select(s => new OrdemServicoServicoPublicoDto(
                s.Id, s.Status.ToString(), s.IniciadoEm, s.ConcluidoEm)),
            os.Insumos.Select(p => new OrdemServicoInsumoPublicoDto(
                p.Id, p.Descricao, p.Quantidade))
        );
    }
}
