namespace AutoReparos.Application.OrdensServicos.UseCases.Aprovacao.Interfaces
{
    public interface IAprovarOrdemServicoUseCase
    {
        Task ExecuteAsync(string token);
    }
}
