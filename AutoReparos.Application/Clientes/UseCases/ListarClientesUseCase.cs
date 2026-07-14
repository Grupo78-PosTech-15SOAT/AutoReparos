using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Clientes.Mappers;
using AutoReparos.Application.Clientes.UseCases.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.Clientes.Repositories;

namespace AutoReparos.Application.Clientes.UseCases
{
    public class ListarClientesUseCase(IClienteRepository repository) : IListarClientesUseCase
    {
        public async Task<PagedResult<ClienteDto>> ExecuteAsync(ClientePagedRequest request)
        {
            var (clientes, total) = await repository.GetAll(request.Nome, request.Skip, request.PageSize);
            var items = clientes.Select(ClienteMapper.ToDto);
            return new PagedResult<ClienteDto>(items, total, request.PageNumber, request.PageSize);
        }
    }
}
