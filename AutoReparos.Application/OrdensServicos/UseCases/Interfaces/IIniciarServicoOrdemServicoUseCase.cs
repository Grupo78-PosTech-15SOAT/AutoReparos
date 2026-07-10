namespace AutoReparos.Application.OrdensServicos.UseCases.Interfaces
{
    public interface IIniciarServicoOrdemServicoUseCase
    {
        Task ExecuteAsync(Guid ordemServicoId, Guid ordemServicoServicoId);
    }
}
