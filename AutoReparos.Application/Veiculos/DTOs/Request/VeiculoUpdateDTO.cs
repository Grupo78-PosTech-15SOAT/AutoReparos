using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.Veiculos.DTOs.Request
{
    public record VeiculoUpdateDTO(
        [Required(ErrorMessage = "Marca é obrigatória.")]
        string Marca,

        [Required(ErrorMessage = "Modelo é obrigatório.")]
        string Modelo,

        [Required(ErrorMessage = "Ano de fabricação é obrigatório.")]
        int AnoFabricacao,

        [Required(ErrorMessage = "Ano modelo é obrigatório.")]
        int AnoModelo
    );
}
