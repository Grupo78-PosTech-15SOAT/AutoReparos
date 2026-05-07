using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.OrdensServicos.Enums;

namespace AutoReparos.Application.OrdensServicos.Services.Interfaces
{
    public interface IOrdemServicoService
    {
        Task<OrdemServicoDto> Create(CriarOrdemServicoDto dto);
        Task<OrdemServicoDetalheDto?> GetById(Guid id);
        Task<OrdemServicoPublicoDetalheDto?> GetPublicById(Guid id);
        Task<PagedResult<OrdemServicoDto>> GetAll(OrdemServicoPagedRequest request);
        Task<PagedResult<OrdemServicoPublicoDto>> GetByDocumentoOuPlaca(OrdemServicoConsultaPagedRequest request);
        Task AdicionarServico(Guid id, AdicionarServicoDto dto);
        Task AdicionarInsumo(Guid id, AdicionarInsumoDto dto);
        Task IniciarDiagnostico(Guid id);
        Task AguardarAprovacao(Guid id);
        Task Aprovar(Guid id);
        Task IniciarServico(Guid ordemServicoId, Guid ordemServicoServicoId);
        Task ConcluirServico(Guid ordemServicoId, Guid ordemServicoServicoId);
        Task Entregar(Guid id);
    }
}
