namespace AutoReparos.Application.OrdensServicos.Services.Interfaces
{
    public interface INotificacaoService
    {
        Task EnviarOrcamento(string token, decimal valorTotal, IEnumerable<string> servicos);
    }
}
