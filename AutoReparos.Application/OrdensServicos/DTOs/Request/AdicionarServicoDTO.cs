using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AutoReparos.Application.OrdensServicos.DTOs.Request
{
    public record AdicionarServicoDTO(
        [Required(ErrorMessage = "Serviço é obrigatório.")]
        Guid ServicoId,

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor cobrado deve ser maior que zero.")]
        decimal ValorCobrado
    );
}
