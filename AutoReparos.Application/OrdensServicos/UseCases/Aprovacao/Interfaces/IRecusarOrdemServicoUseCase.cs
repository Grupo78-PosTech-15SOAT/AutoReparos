namespace AutoReparos.Application.OrdensServicos.UseCases.Aprovacao.Interfaces
{
    public interface IRecusarOrdemServicoUseCase
    {
        Task ExecuteAsync(string token);
    }
}
