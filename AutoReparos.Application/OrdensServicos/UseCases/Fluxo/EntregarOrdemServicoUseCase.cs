using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Fluxo.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AutoReparos.Application.OrdensServicos.UseCases.Fluxo
{
    public class EntregarOrdemServicoUseCase(
        IOrdemServicoRepository repository,
        IClienteRepository clienteRepository,
        INotificacaoService notificacaoService,
        ILogger<EntregarOrdemServicoUseCase> logger) : IEntregarOrdemServicoUseCase
    {
        public async Task ExecuteAsync(Guid id)
        {
            var os = await repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            var statusAnterior = os.Status.ToString();
            os.Entregar();
            await repository.Update(os);

            try
            {
                var cliente = await clienteRepository.GetById(os.ClienteId)
                    ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);
                await notificacaoService.EnviarAtualizacaoStatus(cliente.Email.Endereco, cliente.Nome, os.Id, statusAnterior, os.Status.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao enviar e-mail de entrega de OS {Id}", os.Id);
            }
        }
    }
}
