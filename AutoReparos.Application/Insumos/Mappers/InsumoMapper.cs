using AutoReparos.Application.Insumos.DTOs.Response;
using AutoReparos.Domain.Insumos.Entities;

namespace AutoReparos.Application.Insumos.Mappers
{
    public static class InsumoMapper
    {
        public static InsumoDto ToDto(Insumo insumo) => new(
            insumo.Id, insumo.Nome, insumo.Descricao, insumo.Valor, insumo.QuantidadeEstoque, insumo.CriadoEm, insumo.AtualizadoEm
        );
    }
}
