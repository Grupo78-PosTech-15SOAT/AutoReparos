using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Insumos.Services.Interfaces
{
    public interface IInsumoService
    {
        Task<InsumoDto> Create(CriarInsumoDto dto);
        Task<InsumoDto?> GetById(Guid id);
        Task<PagedResult<InsumoDto>> GetAll(InsumoPagedRequest request);
        Task Update(Guid id, AtualizarInsumoDto dto);
        Task AdicionarEstoque(Guid id, AtualizarEstoqueDto dto);
        Task RemoverEstoque(Guid id, AtualizarEstoqueDto dto);
        Task Delete(Guid id);
    }
}
