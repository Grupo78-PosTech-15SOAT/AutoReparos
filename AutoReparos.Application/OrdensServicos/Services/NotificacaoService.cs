using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Globalization;

namespace AutoReparos.Application.OrdensServicos.Services
{
    public class NotificacaoService : INotificacaoService
    {
        private readonly ILogger<NotificacaoService> _logger;
        private readonly IConfiguration _configuration;

        public NotificacaoService(ILogger<NotificacaoService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task EnviarOrcamento(Guid ordemServicoId, decimal valorTotal)
        {
            var client = new SendGridClient(_configuration["SendGrid:ApiKey"]);

            var from = new EmailAddress(_configuration["SendGrid:FromEmail"], _configuration["SendGrid:FromName"]);

            var to = new EmailAddress(_configuration["SendGrid:ToEmail"], _configuration["SendGrid:ToName"]);

            var subject = $"Orçamento da OS {ordemServicoId}";

            var plainTextContent = $"Olá! O orçamento da sua ordem de serviço ficou em {valorTotal.ToString("C2", new CultureInfo("pt-BR"))}.";

            var htmlContent = $"<strong>Orçamento:</strong> {valorTotal.ToString("C2", new CultureInfo("pt-BR"))}<br />" +
                              $"<strong>Ordem de Serviço:</strong> {ordemServicoId}";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);

            _logger.LogInformation("Email de orçamento enviado. StatusCode: {StatusCode} | OS: {OrdemServicoId}", response.StatusCode, ordemServicoId);
        }
    }
}