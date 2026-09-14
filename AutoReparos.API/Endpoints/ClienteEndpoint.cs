using AutoReparos.API.Controllers;
using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Shared;
using AutoReparos.Application.Veiculos.DTOs.Response;

namespace AutoReparos.API.Endpoints
{
    public static class ClienteEndpoint
    {
        public static void MapClientesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/clientes")
                .WithTags("Clientes");

            // Rota restrita do Portal do Cliente
            group.MapGet("/meus-veiculos", (
                PortalClienteController controller,
                System.Security.Claims.ClaimsPrincipal user,
                CancellationToken cancellationToken) => controller.GetMeusVeiculos(user, cancellationToken))
            .WithName("GetMeusVeiculos")
            .WithSummary("Lista os veículos pertencentes ao cliente autenticado")
            .WithDescription("Endpoint restrito do portal do cliente acessível via token efêmero da Lambda")
            .RequireAuthorization("ClientePolicy")
            .Produces<IEnumerable<VeiculoDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            // Rotas operacionais da oficina (acesso para Mecânico, Atendente, Admin)
            var operacaoGroup = group.MapGroup("/")
                .RequireAuthorization("OperadorOficina");

            operacaoGroup.MapPost("/", (ClienteCreateDto dto, ClienteController controller) => controller.Create(dto))
                .WithName("CreateCliente")
                .WithSummary("Cadastra um novo cliente")
                .WithDescription("Endpoint responsável por cadastrar um novo cliente no sistema")
                .Produces<ClienteDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            operacaoGroup.MapGet("/", (ClienteController controller, [AsParameters] ClientePagedRequest request) => controller.GetAll(request))
            .WithName("GetAllClientes")
            .WithSummary("Lista todos os clientes")
            .WithDescription("Endpoint responsável por retornar todos os clientes ou filtrar por nome quando o parâmetro é informado")
            .Produces<PagedResult<ClienteDto>>(StatusCodes.Status200OK);

            operacaoGroup.MapGet("/{id:guid}", (Guid id, ClienteController controller) => controller.GetById(id))
            .WithName("GetClienteById")
            .WithSummary("Busca cliente por Id")
            .WithDescription("Endpoint responsável por retornar um cliente específico pelo seu id")
            .Produces<ClienteDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            operacaoGroup.MapPut("/{id:guid}", (Guid id, ClienteUpdateDto dto, ClienteController controller) => controller.Update(id, dto))
            .WithName("UpdateCliente")
            .WithSummary("Atualiza os dados de um cliente")
            .WithDescription("Endpoint responsável por atualizar os dados de um cliente pelo seu id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            operacaoGroup.MapDelete("/{id:guid}", (Guid id, ClienteController controller) => controller.Delete(id))
            .WithName("DeleteCliente")
            .WithSummary("Exclui um cliente")
            .WithDescription("Endpoint responsável por excluir um cliente pelo seu id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
