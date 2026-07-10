namespace AutoReparos.Application.Clientes.UseCases.Interfaces
{
    public interface IExcluirClienteUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
