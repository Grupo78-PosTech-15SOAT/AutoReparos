namespace AutoReparos.Application.OrdensServicos.UseCases.Fluxo.Interfaces
{
    public interface IEnviarOrdemServicoParaAprovacaoUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
