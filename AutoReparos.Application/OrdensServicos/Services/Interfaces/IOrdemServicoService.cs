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
        Task<PagedResult<OrdemServicoDTO>> GetAll(Guid? clienteId, Guid? veiculoId, EStatusOrdemServico? status, int pageNumber, int pageSize);
        Task AdicionarServico(Guid id, AdicionarServicoDTO dto);
        Task AdicionarPeca(Guid id, AdicionarPecaDTO dto);
        Task IniciarDiagnostico(Guid id);
        Task AguardarAprovacao(Guid id);
        Task Aprovar(Guid id);
        Task ConcluirServico(Guid ordemServicoId, Guid ordemServicoServicoId);
        Task Entregar(Guid id);
    }
}
