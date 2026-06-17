using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.Servicos.DTOs.Response;
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

        public async Task EnviarOrcamento(string token, decimal valorTotal, IEnumerable<ServicoDto> servicos)
        {
            var baseUrl = _configuration["App:BaseUrl"];
            var aprovarUrl = $"{baseUrl}/api/ordem-servico/aprovar?token={Uri.EscapeDataString(token)}";
            var recusarUrl = $"{baseUrl}/api/ordem-servico/recusar?token={Uri.EscapeDataString(token)}";

            var client = new SendGridClient(_configuration["SendGrid:ApiKey"]);
            var from = new EmailAddress(_configuration["SendGrid:FromEmail"], _configuration["SendGrid:FromName"]);
            var to = new EmailAddress("mateuslecchidev@gmail.com", "Dev");
            var subject = $"Orçamento da Ordem de Serviço";
            var plainTextContent = $"Olá! O orçamento da sua ordem de serviço ficou em {valorTotal.ToString("C2", new CultureInfo("pt-BR"))}.";

            var linhasServicos = string.Join("", servicos.Select(s => $@"
            <tr>
                <td style='border: 1px solid #ddd; padding: 8px;'>{s.Nome}</td>
                <td style='border: 1px solid #ddd; padding: 8px;'>{s.Descricao}</td>
                <td style='border: 1px solid #ddd; padding: 8px;'>{s.ValorTabelado}</td>
            </tr>"));

            var htmlContent = $@"<strong>Orçamento:</strong> {valorTotal.ToString("C2", new CultureInfo("pt-BR"))}<br />
                                 <strong>Serviços da OS:</strong> <br />
                                 <table style='border-collapse: collapse; width: 100%;'>
                                     <thead>
                                         <tr>
                                             <th style='border: 1px solid #ddd; padding: 8px;'>Nome</th>
                                             <th style='border: 1px solid #ddd; padding: 8px;'>Descrição</th>
                                             <th style='border: 1px solid #ddd; padding: 8px;'>Valor</th>
                                         </tr>
                                     </thead>
                                     <tbody>
                                         {linhasServicos}
                                     </tbody>
                                 </table>
                                 <br />
                                 <p>Deseja aprovar o orçamento?</p>
                                 <br />
                                 <a href='{aprovarUrl}' style='background-color:#28a745;color:white;padding:12px 20px;text-decoration:none;border-radius:6px;display:inline-block;margin-right:10px;'>Aceitar</a>
                                 <a href='{recusarUrl}' style='background-color:#dc3545;color:white;padding:12px 20px;text-decoration:none;border-radius:6px;display:inline-block;'>Recusar</a>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);

            _logger.LogInformation("Email de orçamento enviado. StatusCode: {StatusCode}", response.StatusCode);
        }
    }
}