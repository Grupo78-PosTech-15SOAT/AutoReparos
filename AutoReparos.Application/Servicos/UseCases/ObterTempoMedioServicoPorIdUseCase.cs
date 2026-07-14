using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Servicos.Mappers;
using AutoReparos.Application.Servicos.UseCases.Interfaces;
using AutoReparos.Domain.Servicos.Repositories;

namespace AutoReparos.Application.Servicos.UseCases
{
    public class ObterTempoMedioServicoPorIdUseCase(IServicoRepository repository) : IObterTempoMedioServicoPorIdUseCase
    {
        public async Task<TempoMedioServicoDto?> ExecuteAsync(Guid id)
        {
            var result = await repository.GetTempoMedioById(id);
            return result is null ? null : ServicoMapper.ToTempoMedioDto(result.Value);
        }
    }
}
