using AutoReparos.Application.Pecas.DTOs.Request;
using AutoReparos.Application.Pecas.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Pecas.Services.Interfaces
{
    public interface IPecaService
    {
        Task<PecaDTO> Create(CriarPecaDTO dto);
        Task<PecaDTO?> GetById(Guid id);
        Task<PagedResult<PecaDTO>> GetAll(string? nome, int pageNumber, int pageSize);
        Task Update(Guid id, AtualizarPecaDTO dto);
        Task AdicionarEstoque(Guid id, AtualizarEstoqueDTO dto);
        Task RemoverEstoque(Guid id, AtualizarEstoqueDTO dto);
        Task Delete(Guid id);
    }
}
