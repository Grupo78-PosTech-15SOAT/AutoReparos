using AutoReparos.Application.Auth.DTOs.Request;
using AutoReparos.Application.Auth.DTOs.Response;

namespace AutoReparos.Application.Auth.UseCases.Interfaces
{
    public interface ILoginUseCase
    {
        Task<LoginResponseDto?> ExecuteAsync(LoginRequestDto loginRequest);
    }
}
