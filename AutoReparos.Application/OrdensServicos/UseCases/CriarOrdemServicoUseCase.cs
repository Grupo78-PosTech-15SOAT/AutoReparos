using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Mappers;
using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Exceptions;
using AutoReparos.Domain.Veiculos.Repositories;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace AutoReparos.Application.OrdensServicos.UseCases
{
    public class CriarOrdemServicoUseCase(
        IOrdemServicoRepository repository,
        IVeiculoRepository veiculoRepository,
        IInsumoRepository insumoRepository,
        IServicoRepository servicoRepository,
        IClienteRepository clienteRepository,
        INotificacaoService notificacaoService,
        ILogger<CriarOrdemServicoUseCase> logger) : ICriarOrdemServicoUseCase
    {
        public async Task<OrdemServicoDto> ExecuteAsync(CriarOrdemServicoDto dto)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var veiculo = await veiculoRepository.GetById(dto.VeiculoId)
                ?? throw new NotFoundException(ErrorMessages.VeiculoNotFound);

            if (veiculo.ClienteId != dto.ClienteId)
                throw new InvalidVeiculoException("Veículo não pertence ao cliente informado.");

            var cliente = await clienteRepository.GetById(dto.ClienteId)
                ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);

            var ordemServico = new OrdemServico(dto.ClienteId, dto.VeiculoId, dto.Observacao);

            if (dto.Servicos != null)
            {
                foreach (var servicoDto in dto.Servicos)
                {
                    var servico = await servicoRepository.GetById(servicoDto.ServicoId)
                        ?? throw new NotFoundException(ErrorMessages.ServicoNotFound);

                    var item = new OrdemServicoServico(ordemServico.Id, servicoDto.ServicoId, servicoDto.ValorCobrado);
                    ordemServico.AdicionarServico(item);
                }
            }

            if (dto.Insumos != null)
            {
                foreach (var insumoDto in dto.Insumos)
                {
                    decimal valorUnitario;

                    if (insumoDto.Origem == EOrigemInsumo.Estoque)
                    {
                        if (!insumoDto.InsumoId.HasValue)
                            throw new ValidationException("InsumoId é obrigatório para insumos de estoque.");

                        var insumo = await insumoRepository.GetById(insumoDto.InsumoId.Value)
                            ?? throw new NotFoundException(ErrorMessages.InsumoNotFound);

                        insumo.RemoverEstoque(insumoDto.Quantidade);
                        await insumoRepository.Update(insumo);

                        valorUnitario = insumoDto.ValorUnitario ?? insumo.Valor;
                    }
                    else
                    {
                        if (insumoDto.ValorUnitario == null)
                            throw new ValidationException("Valor unitário é obrigatório para insumos de compra específica.");

                        valorUnitario = insumoDto.ValorUnitario.Value;
                    }

                    var item = new OrdemServicoInsumo(ordemServico.Id, insumoDto.InsumoId, insumoDto.Descricao, valorUnitario, insumoDto.Quantidade, insumoDto.Origem);
                    ordemServico.AdicionarInsumo(item);
                }
            }

            await repository.Create(ordemServico);

            scope.Complete();

            try
            {
                await notificacaoService.EnviarAtualizacaoStatus(cliente.Email.Endereco, cliente.Nome, ordemServico.Id, "Nenhum", ordemServico.Status.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao enviar e-mail de notificação de criação de OS {Id}", ordemServico.Id);
            }

            return OrdemServicoMapper.ToDto(ordemServico);
        }
    }
}
