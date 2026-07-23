using AutoReparos.Application.Auth.Services.Interfaces;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Infra.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoReparos.Infra.Identity.Services;

public class JwtService(IOptions<JwtSettings> jwtOptions) : IJwtService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public string GenerateToken(Usuario usuario)
    {
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email.Endereco),
                new Claim(ClaimTypes.Name, usuario.NomeCompleto),
                new Claim(ClaimTypes.Role, usuario.Tipo.ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
