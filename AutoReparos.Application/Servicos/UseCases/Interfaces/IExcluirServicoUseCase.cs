namespace AutoReparos.Application.Servicos.UseCases.Interfaces
{
    public interface IExcluirServicoUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
