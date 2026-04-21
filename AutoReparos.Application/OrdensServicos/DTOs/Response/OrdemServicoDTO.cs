using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record OrdemServicoDTO(
        Guid Id,
        Guid ClienteId,
        Guid VeiculoId,
        string Status,
        string? Observacao,
        decimal ValorTotal,
        DateTime CriadoEm,
        DateTime? IniciadoEm,
        DateTime? FinalizadoEm,
        DateTime? EntregueEm
    );
}
