using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.Insumos.DTOs.Request
{
    public record AtualizarEstoqueDTO(
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero.")]
        int Quantidade
    );
}
