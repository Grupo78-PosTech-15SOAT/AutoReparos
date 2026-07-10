namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces
{
    public interface IConcluirServicoOrdemServicoUseCase
    {
        Task ExecuteAsync(Guid ordemServicoId, Guid ordemServicoServicoId);
    }
}
