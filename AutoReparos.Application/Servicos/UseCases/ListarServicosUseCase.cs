using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Servicos.Mappers;
using AutoReparos.Application.Servicos.UseCases.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.Servicos.Repositories;

namespace AutoReparos.Application.Servicos.UseCases
{
    public class ListarServicosUseCase(IServicoRepository repository) : IListarServicosUseCase
    {
        public async Task<PagedResult<ServicoDto>> ExecuteAsync(ServicoPagedRequest request)
        {
            var (servicos, total) = await repository.GetAll(request.Nome, request.Skip, request.PageSize);
            return new PagedResult<ServicoDto>(servicos.Select(ServicoMapper.ToDto), total, request.PageNumber, request.PageSize);
        }
    }
}
