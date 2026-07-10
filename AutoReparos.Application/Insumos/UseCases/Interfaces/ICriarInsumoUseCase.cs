using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.DTOs.Response;

namespace AutoReparos.Application.Insumos.UseCases.Interfaces
{
    public interface ICriarInsumoUseCase
    {
        Task<InsumoDto> ExecuteAsync(CriarInsumoDto dto);
    }
}
