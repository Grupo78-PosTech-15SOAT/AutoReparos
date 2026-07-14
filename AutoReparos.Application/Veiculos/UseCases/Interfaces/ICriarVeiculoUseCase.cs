using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;

namespace AutoReparos.Application.Veiculos.UseCases.Interfaces
{
    public interface ICriarVeiculoUseCase
    {
        Task<VeiculoDto> ExecuteAsync(VeiculoCreateDto dto);
    }
}
