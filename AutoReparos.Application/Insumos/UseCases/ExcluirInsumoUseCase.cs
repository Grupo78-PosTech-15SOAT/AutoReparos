using AutoReparos.Application.Insumos.UseCases.Interfaces;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Insumos.UseCases
{
    public class ExcluirInsumoUseCase(IInsumoRepository repository) : IExcluirInsumoUseCase
    {
        public async Task ExecuteAsync(Guid id)
        {
            var insumo = await repository.GetById(id)
                ?? throw new NotFoundException("Insumo não encontrado.");

            await repository.Delete(insumo);
        }
    }
}
