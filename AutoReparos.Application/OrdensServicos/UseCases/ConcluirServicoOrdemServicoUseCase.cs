using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AutoReparos.Application.OrdensServicos.UseCases
{
    public class ConcluirServicoOrdemServicoUseCase(
        IOrdemServicoRepository repository,
        IClienteRepository clienteRepository,
        INotificacaoService notificacaoService,
        ILogger<ConcluirServicoOrdemServicoUseCase> logger) : IConcluirServicoOrdemServicoUseCase
    {
        public async Task ExecuteAsync(Guid ordemServicoId, Guid ordemServicoServicoId)
        {
            var os = await repository.GetById(ordemServicoId)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            var statusAnterior = os.Status.ToString();
            os.ConcluirServico(ordemServicoServicoId);
            await repository.Update(os);

            if (statusAnterior != os.Status.ToString())
            {
                try
                {
                    var cliente = await clienteRepository.GetById(os.ClienteId)
                        ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);
                    await notificacaoService.EnviarAtualizacaoStatus(cliente.Email.Endereco, cliente.Nome, os.Id, statusAnterior, os.Status.ToString());
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao enviar e-mail de conclusão de OS {Id}", os.Id);
                }
            }
        }
    }
}
