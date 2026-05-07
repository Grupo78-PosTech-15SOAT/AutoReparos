using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.Insumos.DTOs.Request
{
    public record CriarInsumoDto(
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
