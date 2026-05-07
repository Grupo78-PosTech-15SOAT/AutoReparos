using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;

namespace AutoReparos.Application.Auth.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> Login(LoginRequestDto loginRequest);
    }
}
