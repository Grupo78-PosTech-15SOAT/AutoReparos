using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.OrdensServicos.UseCases
{
    public class AdicionarInsumoOrdemServicoUseCase(IOrdemServicoRepository repository, IInsumoRepository insumoRepository) : IAdicionarInsumoOrdemServicoUseCase
    {
        public async Task ExecuteAsync(Guid id, AdicionarInsumoDto dto)
        {
            var os = await repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            decimal valorUnitario;

            if (dto.Origem == EOrigemInsumo.Estoque)
            {
                var insumo = await insumoRepository.GetById(dto.InsumoId!.Value)
                    ?? throw new NotFoundException(ErrorMessages.InsumoNotFound);

                insumo.RemoverEstoque(dto.Quantidade);
                await insumoRepository.Update(insumo);

                valorUnitario = dto.ValorUnitario ?? insumo.Valor;
            }
            else
            {
                if (dto.ValorUnitario == null)
                    throw new ValidationException("Valor unitário é obrigatório para insumos de compra específica.");

                valorUnitario = dto.ValorUnitario.Value;
            }

            var item = new OrdemServicoInsumo(id, dto.InsumoId, dto.Descricao, valorUnitario, dto.Quantidade, dto.Origem);
            os.AdicionarInsumo(item);
            await repository.Update(os);
        }
    }
}
