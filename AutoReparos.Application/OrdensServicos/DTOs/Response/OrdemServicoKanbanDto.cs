using System;
using System.Collections.Generic;

namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record OrdemServicoKanbanDto(
        Guid Id,
        string NumeroOS,
        string ClienteNome,
        string PlacaVeiculo,
        string ModeloVeiculo,
        string Status,
        decimal ValorTotal,
        IEnumerable<ItemServicoKanbanDto> ItensServico
    );

    public record ItemServicoKanbanDto(Guid Id, bool Concluido);
}
