using AutoReparos.Application.Clientes.DTOs.Request;

namespace AutoReparos.Application.Clientes.UseCases.Interfaces
{
    public interface IAtualizarClienteUseCase
    {
        Task ExecuteAsync(Guid id, ClienteUpdateDto dto);
    }
}
