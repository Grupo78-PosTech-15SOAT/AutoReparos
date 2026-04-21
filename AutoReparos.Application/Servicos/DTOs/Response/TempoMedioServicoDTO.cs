using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Application.Servicos.DTOs.Response
{
    public record TempoMedioServicoDTO(
        Guid ServicoId,
        string NomeServico,
        TimeSpan TempoMedio,
        int TotalExecucoes
    );
}
