using AutoReparos.Application.Servicos.DTOs.Response;

namespace AutoReparos.Application.Servicos.UseCases.Interfaces
{
    public interface IObterServicoPorIdUseCase
    {
        Task<ServicoDto?> ExecuteAsync(Guid id);
    }
}
