using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Insumos.Services.Interfaces
{
    public interface IInsumoService
    {
        Task<InsumoDTO> Create(CriarInsumoDTO dto);
        Task<InsumoDTO?> GetById(Guid id);
        Task<PagedResult<InsumoDTO>> GetAll(string? nome, int pageNumber, int pageSize);
        Task Update(Guid id, AtualizarInsumoDTO dto);
        Task AdicionarEstoque(Guid id, AtualizarEstoqueDTO dto);
        Task RemoverEstoque(Guid id, AtualizarEstoqueDTO dto);
        Task Delete(Guid id);
    }
}
