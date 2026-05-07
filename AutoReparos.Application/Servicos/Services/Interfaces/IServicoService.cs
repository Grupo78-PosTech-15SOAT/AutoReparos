using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Servicos.Services.Interfaces
{
    public interface IServicoService
    {
        Task<ServicoDto> Create(CriarServicoDto dto);
        Task<ServicoDto?> GetById(Guid id);
        Task<PagedResult<ServicoDto>> GetAll(ServicoPagedRequest request);
        Task<IEnumerable<TempoMedioServicoDto>> GetTempoMedio();
        Task<TempoMedioServicoDto?> GetTempoMedioById(Guid id);
        Task Update(Guid id, AtualizarServicoDto dto);
        Task Delete(Guid id);
    }
}
