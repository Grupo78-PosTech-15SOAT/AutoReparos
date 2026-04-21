using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AutoReparos.Application.Pecas.DTOs.Request
{
    public record CriarPecaDTO(
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres.")]
        string Nome,

            string? Descricao,

            [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero.")]
        decimal Valor,

            [Range(0, int.MaxValue, ErrorMessage = "Quantidade não pode ser negativa.")]
        int QuantidadeEstoque
    );
}
