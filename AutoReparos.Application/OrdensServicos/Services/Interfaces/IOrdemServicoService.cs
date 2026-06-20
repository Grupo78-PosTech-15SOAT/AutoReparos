using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.OrdensServicos.Services.Interfaces
{
    public interface IOrdemServicoService
    {
        Task<OrdemServicoDto> Create(CriarOrdemServicoDto dto);
        Task<PagedResult<OrdemServicoDto>> GetAll(OrdemServicoPagedRequest request);
        Task<PagedResult<OrdemServicoDto>> GetFila(PagedRequest request);
        Task<OrdemServicoDetalheDto?> GetById(Guid id);
        Task<OrdemServicoPublicoDetalheDto?> GetPublicById(Guid id);
        Task<PagedResult<OrdemServicoPublicoDto>> GetByDocumentoOuPlaca(OrdemServicoConsultaPagedRequest request);
        Task AdicionarServico(Guid id, AdicionarServicoDto dto);
        Task AdicionarInsumo(Guid id, AdicionarInsumoDto dto);
        Task IniciarDiagnostico(Guid id);
        Task AguardarAprovacao(Guid id);
        Task Aprovar(string token);
        Task Recusar(string token);
        Task IniciarServico(Guid ordemServicoId, Guid ordemServicoServicoId);
        Task ConcluirServico(Guid ordemServicoId, Guid ordemServicoServicoId);
        Task Entregar(Guid id);
    }
}
