namespace AutoReparos.Application.Usuarios.UseCases.Interfaces
{
    public interface IExcluirUsuarioUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
