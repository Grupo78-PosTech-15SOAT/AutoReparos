using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.UseCases.Interfaces;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Insumos.UseCases
{
    public class AdicionarEstoqueUseCase(IInsumoRepository repository) : IAdicionarEstoqueUseCase
    {
        public async Task ExecuteAsync(Guid id, AtualizarEstoqueDto dto)
        {
            var insumo = await repository.GetById(id)
                ?? throw new NotFoundException("Insumo não encontrado.");

            insumo.AdicionarEstoque(dto.Quantidade);
            await repository.Update(insumo);
        }
    }
}
