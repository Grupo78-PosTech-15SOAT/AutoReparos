using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Clientes.UseCases.Interfaces;
using AutoReparos.Application.Shared;

namespace AutoReparos.API.Controllers
{
    public class ClienteController(
        ICriarClienteUseCase criarClienteUseCase,
        IObterClientePorIdUseCase obterClientePorIdUseCase,
        IListarClientesUseCase listarClientesUseCase,
        IAtualizarClienteUseCase atualizarClienteUseCase,
        IExcluirClienteUseCase excluirClienteUseCase)
    {
        public async Task<IResult> Create(ClienteCreateDto dto)
        {
            var cliente = await criarClienteUseCase.ExecuteAsync(dto);
            return Results.CreatedAtRoute("GetClienteById", new { id = cliente.Id }, cliente);
        }

        public async Task<IResult> GetById(Guid id)
        {
            var cliente = await obterClientePorIdUseCase.ExecuteAsync(id);
            return cliente is null ? Results.NotFound() : Results.Ok(cliente);
        }

        public async Task<IResult> GetAll(ClientePagedRequest request)
        {
            var result = await listarClientesUseCase.ExecuteAsync(request);
            return Results.Ok(result);
        }

        public async Task<IResult> Update(Guid id, ClienteUpdateDto dto)
        {
            await atualizarClienteUseCase.ExecuteAsync(id, dto);
            return Results.NoContent();
        }

        public async Task<IResult> Delete(Guid id)
        {
            await excluirClienteUseCase.ExecuteAsync(id);
            return Results.NoContent();
        }
    }
}
