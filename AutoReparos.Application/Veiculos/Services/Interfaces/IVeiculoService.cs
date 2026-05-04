using AutoReparos.Application.Shared;
using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;

namespace AutoReparos.Application.Veiculos.Services.Interfaces
{
    public interface IVeiculoService
    {
        Task<VeiculoDTO> Create(VeiculoCreateDTO dto);

        Task<PagedResult<VeiculoDTO>> GetAll(VeiculoPagedRequest request);
        Task<VeiculoDTO?> GetById(Guid id);
        Task<VeiculoDTO?> GetByPlaca(string placa);

        Task Update(Guid id, VeiculoUpdateDTO dto);
        Task Delete(Guid id);
    }
}