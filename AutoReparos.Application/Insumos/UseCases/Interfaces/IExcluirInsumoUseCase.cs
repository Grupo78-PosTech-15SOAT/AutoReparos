namespace AutoReparos.Application.Insumos.UseCases.Interfaces
{
    public interface IExcluirInsumoUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
