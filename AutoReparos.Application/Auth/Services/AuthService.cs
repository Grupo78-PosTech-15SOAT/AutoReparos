using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;
using AutoReparos.Application.Auth.Services.Interfaces;
using AutoReparos.Domain.Shared.Interfaces;
using AutoReparos.Domain.Usuarios.Repositories;

namespace AutoReparos.Application.Auth.Services
{
    public class AuthService(IAuthRepository authRepository, IJwtService jwtService) : IAuthService
    {
        private readonly IAuthRepository _authRepository = authRepository;
        private readonly IJwtService _jwtService = jwtService;

        public async Task<LoginResponseDTO?> Login(LoginRequestDTO loginRequest)
        {
            var user = await _authRepository.ValidateCredentialsAsync(loginRequest.Email, loginRequest.Password);

            if (user == null)
            {
                return null;
            }

            var token = _jwtService.GenerateToken(user);

            return new LoginResponseDTO(token, user.Email.Endereco, user.NomeCompleto);
        }
    }
}
