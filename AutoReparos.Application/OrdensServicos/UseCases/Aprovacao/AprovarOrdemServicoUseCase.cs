using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Aprovacao.Interfaces;
using AutoReparos.Application.Shared.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AutoReparos.Application.OrdensServicos.UseCases.Aprovacao
{
    public class AprovarOrdemServicoUseCase(
        IOrdemServicoRepository repository,
        IAprovacaoTokenService aprovacaoTokenService,
        IClienteRepository clienteRepository,
        INotificacaoService notificacaoService,
        ILogger<AprovarOrdemServicoUseCase> logger) : IAprovarOrdemServicoUseCase
    {
        public async Task ExecuteAsync(string token)
        {
            var ordemServicoId = aprovacaoTokenService.ValidarToken(token);
            var os = await repository.GetById(ordemServicoId)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            var statusAnterior = os.Status.ToString();
            os.Aprovar();
            await repository.Update(os);

            try
            {
                var cliente = await clienteRepository.GetById(os.ClienteId)
                    ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);
                await notificacaoService.EnviarAtualizacaoStatus(cliente.Email.Endereco, cliente.Nome, os.Id, statusAnterior, os.Status.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao enviar e-mail de aprovação de OS {Id}", os.Id);
            }
        }
    }
}
