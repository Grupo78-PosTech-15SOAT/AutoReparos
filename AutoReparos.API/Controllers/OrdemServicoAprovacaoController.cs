using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;

namespace AutoReparos.API.Controllers
{
    public class OrdemServicoAprovacaoController(
        IAprovarOrdemServicoUseCase aprovarOrdemServicoUseCase,
        IRecusarOrdemServicoUseCase recusarOrdemServicoUseCase)
    {
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
    }
}
