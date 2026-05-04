using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.OrdensServicos.Enums;

namespace AutoReparos.Application.OrdensServicos.Services.Interfaces
{
    public interface IOrdemServicoService
    {
        Task<OrdemServicoDTO> Create(CriarOrdemServicoDTO dto);
        Task<OrdemServicoDetalheDTO?> GetById(Guid id);
        Task<OrdemServicoPublicoDetalheDTO?> GetPublicById(Guid id);
        Task<PagedResult<OrdemServicoDTO>> GetAll(OrdemServicoPagedRequest request);
        Task<PagedResult<OrdemServicoPublicoDTO>> GetByDocumentoOuPlaca(OrdemServicoConsultaPagedRequest request);
        Task AdicionarServico(Guid id, AdicionarServicoDTO dto);
        Task AdicionarInsumo(Guid id, AdicionarInsumoDTO dto);
        Task IniciarDiagnostico(Guid id);
        Task AguardarAprovacao(Guid id);
        Task Aprovar(Guid id);
        Task IniciarServico(Guid ordemServicoId, Guid ordemServicoServicoId);
        Task ConcluirServico(Guid ordemServicoId, Guid ordemServicoServicoId);
        Task Entregar(Guid id);
    }
}
