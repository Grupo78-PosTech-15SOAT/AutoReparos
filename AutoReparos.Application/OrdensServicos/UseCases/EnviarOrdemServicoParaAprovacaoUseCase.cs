using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Shared.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AutoReparos.Application.OrdensServicos.UseCases
{
    public class EnviarOrdemServicoParaAprovacaoUseCase(
        IOrdemServicoRepository repository,
        IServicoRepository servicoRepository,
        IAprovacaoTokenService aprovacaoTokenService,
        IClienteRepository clienteRepository,
        INotificacaoService notificacaoService,
        ILogger<EnviarOrdemServicoParaAprovacaoUseCase> logger) : IEnviarOrdemServicoParaAprovacaoUseCase
    {
        public async Task ExecuteAsync(Guid id)
        {
            var os = await repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            os.AguardarAprovacao();
            await repository.Update(os);

            var token = aprovacaoTokenService.GerarToken(os.Id);

            var servicosDescricao = new List<ServicoDto>();

            foreach (var item in os.Servicos)
            {
                var servico = await servicoRepository.GetById(item.ServicoId);

                if (servico is not null)
                {
                    servicosDescricao.Add(new ServicoDto(
                        servico.Id, servico.Nome, servico.Descricao, item.ValorCobrado, servico.CriadoEm, servico.AtualizadoEm));
                }
            }

            try
            {
                var cliente = await clienteRepository.GetById(os.ClienteId)
                    ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);
                await notificacaoService.EnviarOrcamento(cliente.Email.Endereco, cliente.Nome, token, os.ValorTotal, servicosDescricao);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao enviar e-mail de orçamento de OS {Id}", os.Id);
            }
        }
    }
}
