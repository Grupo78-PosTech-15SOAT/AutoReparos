using AutoReparos.Domain.OrdensServicos.Enums;
using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.OrdensServicos.DTOs.Request
{
    public record AdicionarPecaDTO(
        Guid? PecaId,

        [Required(ErrorMessage = "Descrição é obrigatória.")]
        string Descricao,

        decimal? ValorUnitario,

        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero.")]
        int Quantidade,

        [Required(ErrorMessage = "Origem é obrigatória.")]
        EOrigemPeca Origem
    ) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ValorUnitario.HasValue && ValorUnitario <= 0)
                yield return new ValidationResult(
                    "Valor unitário deve ser maior que zero.",
                    [nameof(ValorUnitario)]);
        }
    };
}
