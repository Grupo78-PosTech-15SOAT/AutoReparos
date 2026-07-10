using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Mappers;
using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces;
using AutoReparos.Domain.Clientes.Entities;
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

namespace AutoReparos.Application.OrdensServicos.UseCases.Core
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

            await ValidarVeiculoDoCliente(dto);

            var cliente = await clienteRepository.GetById(dto.ClienteId)
                ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);

            var ordemServico = new OrdemServico(dto.ClienteId, dto.VeiculoId, dto.Observacao);

            await AdicionarServicos(ordemServico, dto.Servicos);
            await AdicionarInsumos(ordemServico, dto.Insumos);

            await repository.Create(ordemServico);

            scope.Complete();

            await NotificarCriacao(cliente, ordemServico);

            return OrdemServicoMapper.ToDto(ordemServico);
        }

        private async Task ValidarVeiculoDoCliente(CriarOrdemServicoDto dto)
        {
            var veiculo = await veiculoRepository.GetById(dto.VeiculoId)
                ?? throw new NotFoundException(ErrorMessages.VeiculoNotFound);

            if (veiculo.ClienteId != dto.ClienteId)
                throw new InvalidVeiculoException("Veículo não pertence ao cliente informado.");
        }

        private async Task AdicionarServicos(OrdemServico ordemServico, IEnumerable<AdicionarServicoDto>? servicos)
        {
            if (servicos == null)
                return;

            foreach (var servicoDto in servicos)
            {
                _ = await servicoRepository.GetById(servicoDto.ServicoId)
                    ?? throw new NotFoundException(ErrorMessages.ServicoNotFound);

                var item = new OrdemServicoServico(ordemServico.Id, servicoDto.ServicoId, servicoDto.ValorCobrado);
                ordemServico.AdicionarServico(item);
            }
        }

        private async Task AdicionarInsumos(OrdemServico ordemServico, IEnumerable<AdicionarInsumoDto>? insumos)
        {
            if (insumos == null)
                return;

            foreach (var insumoDto in insumos)
            {
                var valorUnitario = await ObterValorUnitario(insumoDto);

                var item = new OrdemServicoInsumo(ordemServico.Id, insumoDto.InsumoId, insumoDto.Descricao, valorUnitario, insumoDto.Quantidade, insumoDto.Origem);
                ordemServico.AdicionarInsumo(item);
            }
        }

        private async Task<decimal> ObterValorUnitario(AdicionarInsumoDto insumoDto)
        {
            if (insumoDto.Origem != EOrigemInsumo.Estoque)
            {
                return insumoDto.ValorUnitario
                    ?? throw new ValidationException("Valor unitário é obrigatório para insumos de compra específica.");
            }

            if (!insumoDto.InsumoId.HasValue)
                throw new ValidationException("InsumoId é obrigatório para insumos de estoque.");

            var insumo = await insumoRepository.GetById(insumoDto.InsumoId.Value)
                ?? throw new NotFoundException(ErrorMessages.InsumoNotFound);

            insumo.RemoverEstoque(insumoDto.Quantidade);
            await insumoRepository.Update(insumo);

            return insumoDto.ValorUnitario ?? insumo.Valor;
        }

        private async Task NotificarCriacao(Cliente cliente, OrdemServico ordemServico)
        {
            try
            {
                await notificacaoService.EnviarAtualizacaoStatus(cliente.Email.Endereco, cliente.Nome, ordemServico.Id, "Nenhum", ordemServico.Status.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao enviar e-mail de notificação de criação de OS {Id}", ordemServico.Id);
            }
        }
    }
}
