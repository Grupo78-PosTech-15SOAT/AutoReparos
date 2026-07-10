using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.DTOs.Response;

namespace AutoReparos.Application.Servicos.UseCases.Interfaces
{
    public interface ICriarServicoUseCase
    {
        Task<ServicoDto> ExecuteAsync(CriarServicoDto dto);
    }
}
