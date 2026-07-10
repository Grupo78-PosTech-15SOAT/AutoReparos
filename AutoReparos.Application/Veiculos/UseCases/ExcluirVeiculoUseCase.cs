using AutoReparos.Application.Veiculos.UseCases.Interfaces;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Repositories;

namespace AutoReparos.Application.Veiculos.UseCases
{
    public class ExcluirVeiculoUseCase(IVeiculoRepository repository) : IExcluirVeiculoUseCase
    {
        public async Task ExecuteAsync(Guid id)
        {
            var veiculo = await repository.GetById(id);

            if (veiculo == null)
                throw new NotFoundException("Veículo não encontrado.");

            await repository.Delete(veiculo);
        }
    }
}
