namespace AutoReparos.Application.OrdensServicos.UseCases.Fluxo.Interfaces
{
    public interface IIniciarDiagnosticoOrdemServicoUseCase
    {
        Task ExecuteAsync(Guid id);
    }
}
