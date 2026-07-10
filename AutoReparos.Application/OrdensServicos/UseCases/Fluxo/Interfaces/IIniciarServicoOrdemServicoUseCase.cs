namespace AutoReparos.Application.OrdensServicos.UseCases.Fluxo.Interfaces
{
    public interface IIniciarServicoOrdemServicoUseCase
    {
        Task ExecuteAsync(Guid ordemServicoId, Guid ordemServicoServicoId);
    }
}
