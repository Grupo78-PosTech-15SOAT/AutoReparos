using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.OrdensServicos.DTOs.Request
{
    public record CriarOrdemServicoDto(
        [Required(ErrorMessage = "Documento é obrigatório.")]
        string DocumentoCliente,

        [Required(ErrorMessage = "Placa é obrigatória.")]
        string PlacaVeiculo,

        string? Observacao,
        IEnumerable<AdicionarServicoDto>? Servicos = null,
        IEnumerable<AdicionarInsumoDto>? Insumos = null
    );
}
