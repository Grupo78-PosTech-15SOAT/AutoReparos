namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces
{
    public interface IIniciarDiagnosticoOrdemServicoUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
