using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;

namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces
{
    public interface ICriarOrdemServicoUseCase
    {
        Task<OrdemServicoDto> ExecuteAsync(CriarOrdemServicoDto dto);
    }
}
