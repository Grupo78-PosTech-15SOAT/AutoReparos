using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.DTOs.Response;
using AutoReparos.Application.Insumos.Mappers;
using AutoReparos.Application.Insumos.UseCases.Interfaces;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Repositories;

namespace AutoReparos.Application.Insumos.UseCases
{
    public class CriarInsumoUseCase(IInsumoRepository repository) : ICriarInsumoUseCase
    {
        public async Task<InsumoDto> ExecuteAsync(CriarInsumoDto dto)
        {
            var insumo = new Insumo(dto.Nome, dto.Descricao, dto.Valor, dto.QuantidadeEstoque);
            await repository.Create(insumo);
            return InsumoMapper.ToDto(insumo);
        }
    }
}
