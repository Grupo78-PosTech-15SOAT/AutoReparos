using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;
using AutoReparos.Application.Auth.Services.Interfaces;
using AutoReparos.Application.Auth.UseCases.Interfaces;
using AutoReparos.Domain.Usuarios.Repositories;

namespace AutoReparos.Application.Auth.UseCases
{
    public class LoginUseCase(IAuthRepository authRepository, IJwtService jwtService) : ILoginUseCase
    {
        public async Task<LoginResponseDto?> ExecuteAsync(LoginRequestDto loginRequest)
        {
            var user = await authRepository.ValidateCredentialsAsync(loginRequest.Email, loginRequest.Password);

            if (user == null)
            {
                return null;
            }

            var token = jwtService.GenerateToken(user);

            return new LoginResponseDto(token, user.Email.Endereco, user.NomeCompleto);
        }
    }
}
