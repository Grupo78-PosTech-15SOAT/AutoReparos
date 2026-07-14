using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;

namespace AutoReparos.Application.Clientes.UseCases.Interfaces
{
    public interface ICriarClienteUseCase
    {
        Task<ClienteDto> ExecuteAsync(ClienteCreateDto dto);
    }
}
