using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces;
using AutoReparos.Application.Shared;

namespace AutoReparos.API.Controllers
{
    public class OrdemServicoController(
        ICriarOrdemServicoUseCase criarOrdemServicoUseCase,
        IListarOrdensServicoUseCase listarOrdensServicoUseCase,
        IListarFilaOrdensServicoUseCase listarFilaOrdensServicoUseCase,
        IObterOrdemServicoPorIdUseCase obterOrdemServicoPorIdUseCase,
        IObterOrdemServicoPublicaPorIdUseCase obterOrdemServicoPublicaPorIdUseCase,
        IConsultarOrdensServicoPorDocumentoOuPlacaUseCase consultarOrdensServicoPorDocumentoOuPlacaUseCase,
        IListarKanbanOrdensServicoUseCase listarKanbanOrdensServicoUseCase)
    {
        public async Task<IResult> GetAll(OrdemServicoPagedRequest request)
        {
            var result = await listarOrdensServicoUseCase.ExecuteAsync(request);
            return Results.Ok(result);
        }

        public async Task<IResult> GetKanban(PagedRequest request)
        {
            var result = await listarKanbanOrdensServicoUseCase.ExecuteAsync(request);
            return Results.Ok(result);
        }

        public async Task<IResult> GetFila(PagedRequest request)
        {
            var result = await listarFilaOrdensServicoUseCase.ExecuteAsync(request);
            return Results.Ok(result);
        }

        public async Task<IResult> GetByDocumentoOuPlaca(OrdemServicoConsultaPagedRequest request)
        {
            var result = await consultarOrdensServicoPorDocumentoOuPlacaUseCase.ExecuteAsync(request);
            return Results.Ok(result);
        }

        public async Task<IResult> GetPublicById(Guid id)
        {
            var os = await obterOrdemServicoPublicaPorIdUseCase.ExecuteAsync(id);
            return os is null ? Results.NotFound() : Results.Ok(os);
        }

        public async Task<IResult> Create(CriarOrdemServicoDto dto)
        {
            var os = await criarOrdemServicoUseCase.ExecuteAsync(dto);
            return Results.CreatedAtRoute("GetOrdemServicoById", new { id = os.Id }, os);
        }

        public async Task<IResult> GetById(Guid id)
        {
            var os = await obterOrdemServicoPorIdUseCase.ExecuteAsync(id);
            return os is null ? Results.NotFound() : Results.Ok(os);
        }
    }
}
