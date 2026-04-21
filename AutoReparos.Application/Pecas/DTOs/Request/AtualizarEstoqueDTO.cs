using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AutoReparos.Application.Pecas.DTOs.Request
{
    public record AtualizarEstoqueDTO(
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero.")]
        int Quantidade
    );
}
