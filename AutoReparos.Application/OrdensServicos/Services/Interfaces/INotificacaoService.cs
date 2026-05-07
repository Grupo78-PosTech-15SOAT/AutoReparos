namespace AutoReparos.Application.OrdensServicos.Services.Interfaces
{
    public interface INotificacaoService
    {
        Task EnviarOrcamento(Guid ordemServicoId, decimal valorTotal);
    }
}
