using AutoReparos.API.Controllers;
using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.API.Endpoints
{
    public static class ClienteEndpoint
    {
        public static void MapClientesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/clientes")
                .WithTags("Clientes")
                .RequireAuthorization();

            group.MapPost("/", (ClienteCreateDto dto, ClienteController controller) => controller.Create(dto))
                .WithName("CreateCliente")
                .WithSummary("Cadastra um novo cliente")
                .WithDescription("Endpoint responsável por cadastrar um novo cliente no sistema")
                .Produces<ClienteDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/", (ClienteController controller, [AsParameters] ClientePagedRequest request) => controller.GetAll(request))
            .WithName("GetAllClientes")
            .WithSummary("Lista todos os clientes")
            .WithDescription("Endpoint responsável por retornar todos os clientes ou filtrar por nome quando o parâmetro é informado")
            .Produces<PagedResult<ClienteDto>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", (Guid id, ClienteController controller) => controller.GetById(id))
            .WithName("GetClienteById")
            .WithSummary("Busca cliente por Id")
            .WithDescription("Endpoint responsável por retornar um cliente específico pelo seu id")
            .Produces<ClienteDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id:guid}", (Guid id, ClienteUpdateDto dto, ClienteController controller) => controller.Update(id, dto))
            .WithName("UpdateCliente")
            .WithSummary("Atualiza os dados de um cliente")
            .WithDescription("Endpoint responsável por atualizar os dados de um cliente pelo seu id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", (Guid id, ClienteController controller) => controller.Delete(id))
            .WithName("DeleteCliente")
            .WithSummary("Exclui um cliente")
            .WithDescription("Endpoint responsável por excluir um cliente pelo seu id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
