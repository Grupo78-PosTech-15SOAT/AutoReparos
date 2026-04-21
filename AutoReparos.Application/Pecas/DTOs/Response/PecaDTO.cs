using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Application.Pecas.DTOs.Response
{
    public record PecaDTO(
        Guid Id,
        string Nome,
        string? Descricao,
        decimal Valor,
        int QuantidadeEstoque,
        DateTime CriadoEm,
        DateTime? AtualizadoEm
    );
}
