using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.Clientes.DTOs.Request
{
    public record ClienteCreateDTO(
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres.")]
        string Nome,

        [Required(ErrorMessage = "Documento é obrigatório.")]
        string Documento,

        [Required(ErrorMessage = "Telefone é obrigatório.")]
        string Telefone,

        [Required(ErrorMessage = "E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        string Email
    );
}
