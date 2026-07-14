namespace AutoReparos.Application.Veiculos.UseCases.Interfaces
{
    public interface IExcluirVeiculoUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
