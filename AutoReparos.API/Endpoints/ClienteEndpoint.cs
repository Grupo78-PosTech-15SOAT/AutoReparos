using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Clientes.Services.Interfaces;
using AutoReparos.Application.Shared;

namespace AutoReparos.API.Endpoints
{
    public static class ClienteEndpoint
    {
        public static void MapClientesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/clientes")
                .WithTags("Clientes");

            group.MapPost("/", async (ClienteCreateDTO dto, IClienteService service) =>
            {
                var cliente = await service.Create(dto);
                return Results.CreatedAtRoute("GetClienteById", new { id = cliente.Id }, cliente);
            })
                .WithName("CreateCliente")
                .WithSummary("Cadastra um novo cliente")
                .WithDescription("Endpoint responsável por cadastrar um novo cliente no sistema")
                .Produces<ClienteDTO>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/", async (IClienteService service, string? nome, int pageNumber = 1, int pageSize = 10) =>
            {
                var request = new PagedRequest
                {
                    Nome = nome,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await service.GetAll(request);

                return Results.Ok(result);
            })
            .WithName("GetAllClientes")
            .WithSummary("Lista todos os clientes")
            .WithDescription("Endpoint responsável por retornar todos os clientes ou filtrar por nome quando o parâmetro é informado")
            .Produces<IEnumerable<ClienteDTO>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", async (Guid id, IClienteService service) =>
            {
                var cliente = await service.GetById(id);
                return cliente is null ? Results.NotFound() : Results.Ok(cliente);
            })
            .WithName("GetClienteById")
            .WithSummary("Busca cliente por Id")
            .WithDescription("Endpoint responsável por retornar um cliente específico pelo seu id")
            .Produces<ClienteDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id:guid}", async (Guid id, ClienteUpdateDTO dto, IClienteService service) =>
            {
                await service.Update(id, dto);
                return Results.NoContent();
            })
            .WithName("UpdateCliente")
            .WithSummary("Atualiza os dados de um cliente")
            .WithDescription("Endpoint responsável por atualizar os dados de um cliente pelo seu id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", async (Guid id, IClienteService service) =>
            {
                await service.Delete(id);
                return Results.NoContent();
            })
            .WithName("DeleteCliente")
            .WithSummary("Exclui um cliente")
            .WithDescription("Endpoint responsável por excluir um cliente pelo seu id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
