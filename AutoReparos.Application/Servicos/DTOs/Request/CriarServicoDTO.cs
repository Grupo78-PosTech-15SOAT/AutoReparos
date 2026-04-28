using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.Servicos.DTOs.Request
{
    public record CriarServicoDTO(
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres.")]
        string Nome,

        string? Descricao,

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor tabelado deve ser maior que zero.")]
        decimal? ValorTabelado
    );
}
