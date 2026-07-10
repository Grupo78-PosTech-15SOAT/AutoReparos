using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.UseCases.Interfaces;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Insumos.UseCases
{
    public class AtualizarInsumoUseCase(IInsumoRepository repository) : IAtualizarInsumoUseCase
    {
        public async Task ExecuteAsync(Guid id, AtualizarInsumoDto dto)
        {
            var insumo = await repository.GetById(id)
                ?? throw new NotFoundException("Insumo não encontrado.");

            insumo.Atualizar(dto.Nome, dto.Descricao, dto.Valor);
            await repository.Update(insumo);
        }
    }
}
