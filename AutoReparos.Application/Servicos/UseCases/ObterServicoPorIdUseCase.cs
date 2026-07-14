using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Servicos.Mappers;
using AutoReparos.Application.Servicos.UseCases.Interfaces;
using AutoReparos.Domain.Servicos.Repositories;

namespace AutoReparos.Application.Servicos.UseCases
{
    public class ObterServicoPorIdUseCase(IServicoRepository repository) : IObterServicoPorIdUseCase
    {
        public async Task<ServicoDto?> ExecuteAsync(Guid id)
        {
            var servico = await repository.GetById(id);
            return servico is null ? null : ServicoMapper.ToDto(servico);
        }
    }
}
