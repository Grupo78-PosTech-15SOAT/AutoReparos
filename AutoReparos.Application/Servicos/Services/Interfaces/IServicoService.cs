using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.Servicos.Services.Interfaces
{
    public interface IServicoService
    {
        Task<ServicoDTO> Create(CriarServicoDTO dto);
        Task<ServicoDTO?> GetById(Guid id);
        Task<PagedResult<ServicoDTO>> GetAll(ServicoPagedRequest request);
        Task<IEnumerable<TempoMedioServicoDTO>> GetTempoMedio();
        Task<TempoMedioServicoDTO?> GetTempoMedioById(Guid id);
        Task Update(Guid id, AtualizarServicoDTO dto);
        Task Delete(Guid id);
    }
}
