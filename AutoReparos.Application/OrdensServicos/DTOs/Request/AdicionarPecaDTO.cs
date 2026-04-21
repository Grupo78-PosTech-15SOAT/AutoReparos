using AutoReparos.Domain.OrdensServicos.Enums;
using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.OrdensServicos.DTOs.Request
{
    public record AdicionarPecaDTO(
        Guid? PecaId,

        [Required(ErrorMessage = "Descrição é obrigatória.")]
        string Descricao,

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor unitário deve ser maior que zero.")]
        decimal ValorUnitario,

        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero.")]
        int Quantidade,

        [Required(ErrorMessage = "Origem é obrigatória.")]
        EOrigemPeca Origem
    );
}
