using AutoReparos.Domain.Usuarios.Entities;

namespace AutoReparos.Application.Auth.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Usuario usuario);
    }
}
