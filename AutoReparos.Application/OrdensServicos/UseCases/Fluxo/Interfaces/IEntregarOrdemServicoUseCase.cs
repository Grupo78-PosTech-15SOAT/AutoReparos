namespace AutoReparos.Application.OrdensServicos.UseCases.Fluxo.Interfaces
{
    public interface IEntregarOrdemServicoUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
