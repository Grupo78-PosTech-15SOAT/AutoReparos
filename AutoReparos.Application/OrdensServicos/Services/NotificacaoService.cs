using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Shared.Interfaces;
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
        private readonly IAppUrlProvider _appUrlProvider;

        public NotificacaoService(ILogger<NotificacaoService> logger, IConfiguration configuration, IAppUrlProvider appUrlProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _appUrlProvider = appUrlProvider;
        }

        public async Task EnviarOrcamento(string emailDestinatario, string nomeDestinatario, string token, decimal valorTotal, IEnumerable<ServicoDto> servicos)
        {
            var baseUrl = _appUrlProvider.GetBaseUrl();
            var aprovarUrl = $"{baseUrl}/api/ordem-servico/aprovar?token={Uri.EscapeDataString(token)}";
            var recusarUrl = $"{baseUrl}/api/ordem-servico/recusar?token={Uri.EscapeDataString(token)}";

            var client = new SendGridClient(_configuration["SendGrid:ApiKey"]);
            var from = new EmailAddress(_configuration["SendGrid:FromEmail"], _configuration["SendGrid:FromName"]);
            var to = new EmailAddress(emailDestinatario, nomeDestinatario);
            var subject = "Orçamento da Ordem de Serviço";
            var plainTextContent = $"Olá, {nomeDestinatario}! O valor do orçamento da ordem de serviço ficou em {valorTotal.ToString("C2", new CultureInfo("pt-BR"))}.";

            var linhasServicos = string.Join("", servicos.Select(s => $@"
            <tr>
                <td style='border: 1px solid #e2e8f0; padding: 12px; color: #334155; font-size: 14px;'>{s.Nome}</td>
                <td style='border: 1px solid #e2e8f0; padding: 12px; color: #334155; font-size: 14px;'>{s.Descricao}</td>
                <td style='border: 1px solid #e2e8f0; padding: 12px; color: #334155; font-size: 14px; text-align: right;'>{s.ValorTabelado?.ToString("C2", new CultureInfo("pt-BR")) ?? "R$ 0,00"}</td>
            </tr>"));

            var htmlContent = $@"
<div style='font-family: ""Outfit"", ""Inter"", Helvetica, Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px; background-color: #ffffff;'>
    <div style='text-align: center; margin-bottom: 24px; padding-bottom: 20px; border-bottom: 2px solid #f1f5f9;'>
        <h2 style='color: #1e293b; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: -0.025em;'>AutoReparos</h2>
        <p style='color: #64748b; margin: 4px 0 0 0; font-size: 14px;'>Orçamento da Ordem de Serviço</p>
    </div>

    <div style='margin-bottom: 24px;'>
        <p style='color: #334155; font-size: 16px; line-height: 1.6; margin: 0;'>Olá, <strong>{nomeDestinatario}</strong>,</p>
        <p style='color: #334155; font-size: 16px; line-height: 1.6; margin: 8px 0 0 0;'>
            O diagnóstico do seu veículo foi concluído e o orçamento ficou em <strong style='font-size: 18px; color: #2563eb;'>{valorTotal.ToString("C2", new CultureInfo("pt-BR"))}</strong>.
        </p>
    </div>

    <div style='margin-bottom: 24px;'>
        <h3 style='color: #1e293b; font-size: 16px; font-weight: 600; margin: 0 0 12px 0;'>Serviços diagnosticados:</h3>
        <table style='border-collapse: collapse; width: 100%; border: 1px solid #e2e8f0;'>
            <thead>
                <tr style='background-color: #f8fafc;'>
                    <th style='border: 1px solid #e2e8f0; padding: 12px; text-align: left; color: #475569; font-weight: 600; font-size: 14px;'>Nome</th>
                    <th style='border: 1px solid #e2e8f0; padding: 12px; text-align: left; color: #475569; font-weight: 600; font-size: 14px;'>Descrição</th>
                    <th style='border: 1px solid #e2e8f0; padding: 12px; text-align: right; color: #475569; font-weight: 600; font-size: 14px;'>Valor</th>
                </tr>
            </thead>
            <tbody>
                {linhasServicos}
            </tbody>
        </table>
    </div>

    <div style='background-color: #f8fafc; border-radius: 8px; padding: 20px; margin-bottom: 24px; border: 1px solid #f1f5f9; text-align: center;'>
        <p style='color: #334155; font-size: 16px; font-weight: 500; margin: 0 0 16px 0;'>Deseja aprovar o orçamento?</p>
        <div style='text-align: center;'>
            <a href='{aprovarUrl}' style='background-color:#22c55e;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;display:inline-block;margin-right:12px;font-weight:600;font-size:15px;box-shadow: 0 2px 4px rgba(34, 197, 94, 0.2);'>Aprovar Orçamento</a>
            <a href='{recusarUrl}' style='background-color:#ef4444;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;display:inline-block;font-weight:600;font-size:15px;box-shadow: 0 2px 4px rgba(239, 68, 68, 0.2);'>Recusar Orçamento</a>
        </div>
    </div>

    <div style='margin-top: 32px; padding-top: 20px; border-top: 1px solid #e2e8f0; text-align: center; color: #94a3b8; font-size: 12px;'>
        <p style='margin: 0;'>Este é um e-mail automático enviado pelo sistema AutoReparos.</p>
        <p style='margin: 4px 0 0 0;'>FIAP - Pós-Graduação em Arquitetura de Software</p>
    </div>
</div>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            _logger.LogInformation("Email de orçamento enviado. StatusCode: {StatusCode}", response.StatusCode);
        }

        public async Task EnviarAtualizacaoStatus(string emailDestinatario, string nomeDestinatario, Guid ordemServicoId, string statusAnterior, string novoStatus)
        {
            var client = new SendGridClient(_configuration["SendGrid:ApiKey"]);
            var from = new EmailAddress(_configuration["SendGrid:FromEmail"], _configuration["SendGrid:FromName"]);
            var to = new EmailAddress(emailDestinatario, nomeDestinatario);
            var subject = $"Atualização de Status da Ordem de Serviço #{ordemServicoId.ToString().Substring(0, 8).ToUpper()}";

            var labelAnterior = TraduzirStatus(statusAnterior);
            var labelNovo = TraduzirStatus(novoStatus);

            var plainTextContent = $"Olá, {nomeDestinatario}! O status da sua ordem de serviço foi atualizado de '{labelAnterior}' para '{labelNovo}'.";

            string mensagemAdicional = "";
            if (statusAnterior == "AguardandoAprovacao" && novoStatus == "EmDiagnostico")
            {
                mensagemAdicional = "<p style='color: #ef4444; font-weight: 500; font-size: 15px; margin: 16px 0 0 0; text-align: center;'>Identificamos que o orçamento foi recusado. Nossa equipe já foi notificada e está revisando os itens do diagnóstico para encontrar a melhor solução para você. Entraremos em contato em breve!</p>";
            }
            else if (novoStatus == "EmExecucao")
            {
                mensagemAdicional = "<p style='color: #22c55e; font-weight: 500; font-size: 15px; margin: 16px 0 0 0; text-align: center;'>Seu orçamento foi aprovado e nossos técnicos já iniciaram os trabalhos no seu veículo!</p>";
            }
            else if (novoStatus == "Finalizada")
            {
                mensagemAdicional = "<p style='color: #22c55e; font-weight: 500; font-size: 15px; margin: 16px 0 0 0; text-align: center;'>Todos os serviços foram concluídos com sucesso! Seu veículo está pronto e aguardando a entrega.</p>";
            }

            var htmlContent = $@"
<div style='font-family: ""Outfit"", ""Inter"", Helvetica, Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px; background-color: #ffffff;'>
    <div style='text-align: center; margin-bottom: 24px; padding-bottom: 20px; border-bottom: 2px solid #f1f5f9;'>
        <h2 style='color: #1e293b; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: -0.025em;'>AutoReparos</h2>
        <p style='color: #64748b; margin: 4px 0 0 0; font-size: 14px;'>Atualização da Ordem de Serviço</p>
    </div>
    
    <div style='margin-bottom: 24px;'>
        <p style='color: #334155; font-size: 16px; line-height: 1.6; margin: 0;'>Olá, <strong>{nomeDestinatario}</strong>,</p>
        <p style='color: #334155; font-size: 16px; line-height: 1.6; margin: 8px 0 0 0;'>
            Gostaríamos de informar que a sua Ordem de Serviço <strong>#{ordemServicoId.ToString().Substring(0, 8).ToUpper()}</strong> mudou de status:
        </p>
    </div>

    <div style='background-color: #f8fafc; border-radius: 8px; padding: 20px; margin-bottom: 24px; border: 1px solid #f1f5f9; text-align: center;'>
        <div style='display: inline-block; vertical-align: middle; padding: 8px 16px; background-color: #e2e8f0; border-radius: 20px; color: #475569; font-weight: 600; font-size: 14px;'>
            {labelAnterior}
        </div>
        <div style='display: inline-block; vertical-align: middle; margin: 0 12px; color: #94a3b8; font-size: 18px; font-weight: bold;'>
            ➔
        </div>
        <div style='display: inline-block; vertical-align: middle; padding: 8px 16px; background-color: #3b82f6; border-radius: 20px; color: #ffffff; font-weight: 600; font-size: 14px; box-shadow: 0 2px 4px rgba(59, 130, 246, 0.2);'>
            {labelNovo}
        </div>
        {mensagemAdicional}
    </div>

    <div style='margin-top: 32px; padding-top: 20px; border-top: 1px solid #e2e8f0; text-align: center; color: #94a3b8; font-size: 12px;'>
        <p style='margin: 0;'>Este é um e-mail automático enviado pelo sistema AutoReparos.</p>
        <p style='margin: 4px 0 0 0;'>FIAP - Pós-Graduação em Arquitetura de Software</p>
    </div>
</div>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            _logger.LogInformation("Email de atualização de status enviado. StatusCode: {StatusCode}", response.StatusCode);
        }

        private static string TraduzirStatus(string status)
        {
            return status switch
            {
                "Recebida" => "Recebida",
                "EmDiagnostico" => "Em Diagnóstico",
                "AguardandoAprovacao" => "Aguardando Aprovação",
                "EmExecucao" => "Em Execução",
                "Finalizada" => "Finalizada",
                "Entregue" => "Entregue",
                _ => status
            };
        }
    }
}