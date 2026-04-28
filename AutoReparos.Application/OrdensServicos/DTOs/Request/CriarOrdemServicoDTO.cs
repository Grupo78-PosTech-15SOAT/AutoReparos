using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.OrdensServicos.DTOs.Request
{
    public record CriarOrdemServicoDTO(
        [Required(ErrorMessage = "Cliente é obrigatório.")]
        Guid ClienteId,

        [Required(ErrorMessage = "Veículo é obrigatório.")]
        Guid VeiculoId,

        string? Observacao
    );
}
