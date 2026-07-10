using AutoReparos.Application.Clientes.DTOs.Response;

namespace AutoReparos.Application.Clientes.UseCases.Interfaces
{
    public interface IObterClientePorIdUseCase
    {
        Task<ClienteDto?> ExecuteAsync(Guid id);
    }
}
