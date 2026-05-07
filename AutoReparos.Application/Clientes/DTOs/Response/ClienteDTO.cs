namespace AutoReparos.Application.Clientes.DTOs.Response
{
    public record ClienteDto(
        Guid Id,
        string Nome,
        string Documento,
        string TipoDocumento,
        string Telefone,
        string Email
    );
}
