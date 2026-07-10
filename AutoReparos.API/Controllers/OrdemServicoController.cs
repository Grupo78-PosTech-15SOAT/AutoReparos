using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;
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
        IAdicionarServicoOrdemServicoUseCase adicionarServicoOrdemServicoUseCase,
        IAdicionarInsumoOrdemServicoUseCase adicionarInsumoOrdemServicoUseCase,
        IIniciarDiagnosticoOrdemServicoUseCase iniciarDiagnosticoOrdemServicoUseCase,
        IEnviarOrdemServicoParaAprovacaoUseCase enviarOrdemServicoParaAprovacaoUseCase,
        IAprovarOrdemServicoUseCase aprovarOrdemServicoUseCase,
        IRecusarOrdemServicoUseCase recusarOrdemServicoUseCase,
        IIniciarServicoOrdemServicoUseCase iniciarServicoOrdemServicoUseCase,
        IConcluirServicoOrdemServicoUseCase concluirServicoOrdemServicoUseCase,
        IEntregarOrdemServicoUseCase entregarOrdemServicoUseCase)
    {
        public async Task<IResult> GetAll(OrdemServicoPagedRequest request)
        {
            var result = await listarOrdensServicoUseCase.ExecuteAsync(request);
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

        public async Task<IResult> AdicionarServico(Guid id, AdicionarServicoDto dto)
        {
            await adicionarServicoOrdemServicoUseCase.ExecuteAsync(id, dto);
            return Results.NoContent();
        }

        public async Task<IResult> AdicionarInsumo(Guid id, AdicionarInsumoDto dto)
        {
            await adicionarInsumoOrdemServicoUseCase.ExecuteAsync(id, dto);
            return Results.NoContent();
        }

        public async Task<IResult> IniciarDiagnostico(Guid id)
        {
            await iniciarDiagnosticoOrdemServicoUseCase.ExecuteAsync(id);
            return Results.NoContent();
        }

        public async Task<IResult> EnviarParaAprovacao(Guid id)
        {
            await enviarOrdemServicoParaAprovacaoUseCase.ExecuteAsync(id);
            return Results.NoContent();
        }

        public async Task<IResult> Aprovar(string token)
        {
            await aprovarOrdemServicoUseCase.ExecuteAsync(token);
            return Results.Content("Orçamento aprovado");
        }

        public async Task<IResult> Recusar(string token)
        {
            await recusarOrdemServicoUseCase.ExecuteAsync(token);
            return Results.Content("Orçamento recusado");
        }

        public async Task<IResult> IniciarServico(Guid id, Guid servicoId)
        {
            await iniciarServicoOrdemServicoUseCase.ExecuteAsync(id, servicoId);
            return Results.NoContent();
        }

        public async Task<IResult> ConcluirServico(Guid id, Guid servicoId)
        {
            await concluirServicoOrdemServicoUseCase.ExecuteAsync(id, servicoId);
            return Results.NoContent();
        }

        public async Task<IResult> Entregar(Guid id)
        {
            await entregarOrdemServicoUseCase.ExecuteAsync(id);
            return Results.NoContent();
        }
    }
}
