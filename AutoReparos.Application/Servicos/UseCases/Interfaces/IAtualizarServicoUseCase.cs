using AutoReparos.Application.Servicos.DTOs.Request;

namespace AutoReparos.Application.Servicos.UseCases.Interfaces
{
    public interface IAtualizarServicoUseCase
    {
        Task ExecuteAsync(Guid id, AtualizarServicoDto dto);
    }
}
