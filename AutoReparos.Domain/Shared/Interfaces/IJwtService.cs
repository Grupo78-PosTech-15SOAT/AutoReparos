using AutoReparos.Domain.Usuarios.Entities;

namespace AutoReparos.Domain.Shared.Interfaces;

public interface IJwtService
{
    string GenerateToken(Usuario usuario);
}
