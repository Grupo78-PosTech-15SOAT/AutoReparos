using AutoReparos.Application.Shared;
using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;

namespace AutoReparos.Application.Veiculos.Services.Interfaces
{
    public interface IVeiculoService
    {
        Task<VeiculoDto> Create(VeiculoCreateDto dto);

        Task<PagedResult<VeiculoDto>> GetAll(VeiculoPagedRequest request);
        Task<VeiculoDto?> GetById(Guid id);
        Task<VeiculoDto?> GetByPlaca(string placa);

        Task Update(Guid id, VeiculoUpdateDto dto);
        Task Delete(Guid id);
    }
}