namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces
{
    public interface IEntregarOrdemServicoUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
