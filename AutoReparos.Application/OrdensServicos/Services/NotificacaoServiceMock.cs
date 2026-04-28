using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace AutoReparos.Application.OrdensServicos.Services
{
    public class NotificacaoServiceMock : INotificacaoService
    {
        private readonly ILogger<NotificacaoServiceMock> _logger;

        public NotificacaoServiceMock(ILogger<NotificacaoServiceMock> logger)
        {
            _logger = logger;
        }

        public Task EnviarOrcamento(Guid ordemServicoId, decimal valorTotal)
        {
            _logger.LogInformation(
               "[MOCK] Orçamento enviado ao cliente - OS: {OrdemServicoId} | Valor Total: {ValorTotal}",
               ordemServicoId,
               valorTotal.ToString("C2", new CultureInfo("pt-BR")));

            return Task.CompletedTask;
        }
    }
}
