using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.UseCases.Fluxo.Interfaces;

namespace AutoReparos.API.Controllers
{
    public class OrdemServicoFluxoController(
        IAdicionarServicoOrdemServicoUseCase adicionarServicoOrdemServicoUseCase,
        IAdicionarInsumoOrdemServicoUseCase adicionarInsumoOrdemServicoUseCase,
        IIniciarDiagnosticoOrdemServicoUseCase iniciarDiagnosticoOrdemServicoUseCase,
        IEnviarOrdemServicoParaAprovacaoUseCase enviarOrdemServicoParaAprovacaoUseCase,
        IIniciarServicoOrdemServicoUseCase iniciarServicoOrdemServicoUseCase,
        IConcluirServicoOrdemServicoUseCase concluirServicoOrdemServicoUseCase,
        IEntregarOrdemServicoUseCase entregarOrdemServicoUseCase)
    {
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
