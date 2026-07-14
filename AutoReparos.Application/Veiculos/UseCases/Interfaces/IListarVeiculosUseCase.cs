using AutoReparos.Application.Shared;
using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;

namespace AutoReparos.Application.Veiculos.UseCases.Interfaces
{
    public interface IListarVeiculosUseCase
    {
        Task<PagedResult<VeiculoDto>> ExecuteAsync(VeiculoPagedRequest request);
    }
}
