using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;

namespace AutoReparos.Application.Veiculos.Services.Interfaces
{
    public interface IVeiculoService
    {
        Task<VeiculoDTO> Create(VeiculoCreateDTO dto);
        Task<VeiculoDTO?> GetById(Guid id);
        Task<IEnumerable<VeiculoDTO>> GetByClienteId(Guid clienteId);
        Task Update(Guid id, VeiculoUpdateDTO dto);
        Task Delete(Guid id);
    }
}