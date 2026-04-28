using AutoReparos.Application.Auth.Configurations;
using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;
using AutoReparos.Application.Auth.Services.Interfaces;
using AutoReparos.Domain.Usuarios.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoReparos.Application.Auth.Services
{
    public class AuthService(UserManager<Usuario> userManager, IOptions<JwtSettings> jwtOptions) : IAuthService
    {
        private readonly UserManager<Usuario> _userManager = userManager;
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;

        public async Task<LoginResponseDTO?> Login(LoginRequestDTO loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                return null;
            }

            var token = GenerateJwtToken(user);

            return new LoginResponseDTO(token, user.Email!, user.NomeCompleto);
        }

        private string GenerateJwtToken(Usuario user)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.Name, user.NomeCompleto)
                ]),
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
