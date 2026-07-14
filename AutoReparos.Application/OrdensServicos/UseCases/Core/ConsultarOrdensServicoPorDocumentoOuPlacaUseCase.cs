using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Mappers;
using AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.OrdensServicos.Repositories;

namespace AutoReparos.Application.OrdensServicos.UseCases.Core
{
    public class ConsultarOrdensServicoPorDocumentoOuPlacaUseCase(IOrdemServicoRepository repository) : IConsultarOrdensServicoPorDocumentoOuPlacaUseCase
    {
        public async Task<PagedResult<OrdemServicoPublicoDto>> ExecuteAsync(OrdemServicoConsultaPagedRequest request)
        {
            var (items, total) = await repository.GetByDocumentoOuPlaca(request.Documento, request.Placa, request.Skip, request.PageSize);
            return new PagedResult<OrdemServicoPublicoDto>(items.Select(OrdemServicoMapper.ToPublicDto), total, request.PageNumber, request.PageSize);
        }
    }
}
