namespace AutoReparos.Application.Auth.DTOs.Response
{
    public record LoginResponseDTO(string Token, string Email, string NomeCompleto);
}
