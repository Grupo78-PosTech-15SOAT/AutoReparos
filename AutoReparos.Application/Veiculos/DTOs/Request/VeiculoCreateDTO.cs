using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.Veiculos.DTOs.Request
{
    public record VeiculoCreateDto(
        [Required(ErrorMessage = "Cliente é obrigatório.")]
        Guid ClienteId,

        [Required(ErrorMessage = "Marca é obrigatória.")]
        string Marca,

        [Required(ErrorMessage = "Modelo é obrigatório.")]
        string Modelo,

        [Required(ErrorMessage = "Ano de fabricação é obrigatório.")]
        int AnoFabricacao,

        [Required(ErrorMessage = "Ano modelo é obrigatório.")]
        int AnoModelo,

        [Required(ErrorMessage = "Placa é obrigatória.")]
        string Placa,

        [Required(ErrorMessage = "Chassi é obrigatório.")]
        string Chassi,

        [Required(ErrorMessage = "Renavam é obrigatório.")]
        string Renavam
    );
}