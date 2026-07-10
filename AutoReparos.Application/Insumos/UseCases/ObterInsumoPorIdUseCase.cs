using AutoReparos.Application.Insumos.DTOs.Response;
using AutoReparos.Application.Insumos.Mappers;
using AutoReparos.Application.Insumos.UseCases.Interfaces;
using AutoReparos.Domain.Insumos.Repositories;

namespace AutoReparos.Application.Insumos.UseCases
{
    public class ObterInsumoPorIdUseCase(IInsumoRepository repository) : IObterInsumoPorIdUseCase
    {
        public async Task<InsumoDto?> ExecuteAsync(Guid id)
        {
            var insumo = await repository.GetById(id);
            return insumo is null ? null : InsumoMapper.ToDto(insumo);
        }
    }
}
