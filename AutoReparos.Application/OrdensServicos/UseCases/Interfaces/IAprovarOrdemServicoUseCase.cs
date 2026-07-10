namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces
{
    public interface IAprovarOrdemServicoUseCase
    {
        Task ExecuteAsync(string token);
    }
}
