using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Servicos.Mappers;
using AutoReparos.Application.Servicos.UseCases.Interfaces;
using AutoReparos.Domain.Servicos.Repositories;

namespace AutoReparos.Application.Servicos.UseCases
{
    public class ObterTempoMedioServicosUseCase(IServicoRepository repository) : IObterTempoMedioServicosUseCase
    {
        public async Task<IEnumerable<TempoMedioServicoDto>> ExecuteAsync()
        {
            var result = await repository.GetTempoMedio();
            return result.Select(ServicoMapper.ToTempoMedioDto);
        }
    }
}
