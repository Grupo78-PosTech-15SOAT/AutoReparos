namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces
{
    public interface IEnviarOrdemServicoParaAprovacaoUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
