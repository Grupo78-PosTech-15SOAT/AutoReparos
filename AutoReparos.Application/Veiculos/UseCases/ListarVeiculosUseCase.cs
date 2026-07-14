using AutoReparos.Application.Shared;
using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;
using AutoReparos.Application.Veiculos.Mappers;
using AutoReparos.Application.Veiculos.UseCases.Interfaces;
using AutoReparos.Domain.Veiculos.Repositories;

namespace AutoReparos.Application.Veiculos.UseCases
{
    public class ListarVeiculosUseCase(IVeiculoRepository repository) : IListarVeiculosUseCase
    {
        public async Task<PagedResult<VeiculoDto>> ExecuteAsync(VeiculoPagedRequest request)
        {
            var (items, total) = await repository.GetAll(request.ClienteId, request.Skip, request.PageSize);
            return new PagedResult<VeiculoDto>(items.Select(VeiculoMapper.ToDto), total, request.PageNumber, request.PageSize);
        }
    }
}
