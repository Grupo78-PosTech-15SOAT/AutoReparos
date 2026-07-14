using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces
{
    public interface IConsultarOrdensServicoPorDocumentoOuPlacaUseCase
    {
        Task<PagedResult<OrdemServicoPublicoDto>> ExecuteAsync(OrdemServicoConsultaPagedRequest request);
    }
}
