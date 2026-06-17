using AutoReparos.Application.Servicos.DTOs.Response;

namespace AutoReparos.Application.OrdensServicos.Services.Interfaces
{
    public interface INotificacaoService
    {
        Task EnviarOrcamento(string token, decimal valorTotal, IEnumerable<ServicoDto> servicos);
    }
}
