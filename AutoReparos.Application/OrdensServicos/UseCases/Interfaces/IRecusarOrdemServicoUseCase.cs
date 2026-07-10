namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces
{
    public interface IRecusarOrdemServicoUseCase
    {
        Task ExecuteAsync(string token);
    }
}
