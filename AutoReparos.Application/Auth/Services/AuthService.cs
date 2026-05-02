using AutoReparos.Application.Auth.Configurations;
using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;
using AutoReparos.Application.Auth.Services.Interfaces;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoReparos.Application.Auth.Services
{
    public class AuthService(IAuthRepository authRepository, IOptions<JwtSettings> jwtOptions) : IAuthService
    {
        private readonly IAuthRepository _authRepository = authRepository;
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;

        public async Task<LoginResponseDTO?> Login(LoginRequestDTO loginRequest)
        {
            var user = await _authRepository.ValidateCredentialsAsync(loginRequest.Email, loginRequest.Password);

            if (user == null)
            {
                return null;
            }

            var token = GenerateJwtToken(user);

            return new LoginResponseDTO(token, user.Email.Endereco, user.NomeCompleto);
        }

        private string GenerateJwtToken(Usuario user)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email.Endereco),
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
