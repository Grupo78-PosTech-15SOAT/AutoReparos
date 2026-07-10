using AutoReparos.API.Controllers;
using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.API.Endpoints
{
    public static class InsumosEndpoints
    {
        public static void MapInsumosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/insumos")
                .WithTags("Insumos")
                .RequireAuthorization();

            group.MapPost("/", (CriarInsumoDto dto, InsumoController controller) => controller.Create(dto))
            .WithName("CreateInsumo")
            .WithSummary("Cadastra um novo insumo")
            .Produces<InsumoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/{id:guid}", (Guid id, InsumoController controller) => controller.GetById(id))
            .WithName("GetInsumoById")
            .WithSummary("Busca um insumo por ID")
            .Produces<InsumoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/", (InsumoController controller, [AsParameters] InsumoPagedRequest request) => controller.GetAll(request))
            .WithName("GetAllInsumos")
            .WithSummary("Lista paginada de todos os insumos")
            .Produces<PagedResult<InsumoDto>>(StatusCodes.Status200OK);

            group.MapPut("/{id:guid}", (Guid id, AtualizarInsumoDto dto, InsumoController controller) => controller.Update(id, dto))
            .WithName("UpdateInsumo")
            .WithSummary("Atualiza os dados de um insumo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/adicionar-estoque", (
                Guid id,
                AtualizarEstoqueDto dto,
                InsumoController controller) => controller.AdicionarEstoque(id, dto))
            .WithName("AdicionarEstoque")
            .WithSummary("Adiciona quantidade ao estoque de um insumo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/remover-estoque", (
                Guid id,
                AtualizarEstoqueDto dto,
                InsumoController controller) => controller.RemoverEstoque(id, dto))
            .WithName("RemoverEstoque")
            .WithSummary("Remove quantidade do estoque de um insumo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", (Guid id, InsumoController controller) => controller.Delete(id))
            .WithName("DeleteInsumo")
            .WithSummary("Remove um insumo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
