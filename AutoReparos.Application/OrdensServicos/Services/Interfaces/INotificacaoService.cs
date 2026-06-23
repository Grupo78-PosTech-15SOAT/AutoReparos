using AutoReparos.Application.Servicos.DTOs.Response;

namespace AutoReparos.Application.OrdensServicos.Services.Interfaces
{
    public interface INotificacaoService
    {
        Task EnviarOrcamento(string emailDestinatario, string nomeDestinatario, string token, decimal valorTotal, IEnumerable<ServicoDto> servicos);
        Task EnviarAtualizacaoStatus(string emailDestinatario, string nomeDestinatario, Guid ordemServicoId, string statusAnterior, string novoStatus);
    }
}
