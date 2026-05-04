using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.DTOs.Response;
using AutoReparos.Application.Insumos.Services.Interfaces;
using AutoReparos.Application.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AutoReparos.API.Endpoints
{
    public static class InsumosEndpoints
    {
        public static void MapInsumosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/insumos")
                .WithTags("Insumos")
                .RequireAuthorization();

            group.MapPost("/", async (CriarInsumoDTO dto, IInsumoService service) =>
            {
                var insumo = await service.Create(dto);
                return Results.CreatedAtRoute("GetInsumoById", new { id = insumo.Id }, insumo);
            })
            .WithName("CreateInsumo")
            .WithSummary("Cadastra um novo insumo")
            .Produces<InsumoDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/{id:guid}", async (Guid id, IInsumoService service) =>
            {
                var insumo = await service.GetById(id);
                return insumo is null ? Results.NotFound() : Results.Ok(insumo);
            })
            .WithName("GetInsumoById")
            .WithSummary("Busca um insumo por ID")
            .Produces<InsumoDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/", async (
                IInsumoService service,
                [AsParameters] InsumoPagedRequest request) =>
            {
                var result = await service.GetAll(request);
                return Results.Ok(result);
            })
            .WithName("GetAllInsumos")
            .WithSummary("Lista paginada de todos os insumos")
            .Produces<PagedResult<InsumoDTO>>(StatusCodes.Status200OK);

            group.MapPut("/{id:guid}", async (Guid id, AtualizarInsumoDTO dto, IInsumoService service) =>
            {
                await service.Update(id, dto);
                return Results.NoContent();
            })
            .WithName("UpdateInsumo")
            .WithSummary("Atualiza os dados de um insumo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/adicionar-estoque", async (
                Guid id,
                AtualizarEstoqueDTO dto,
                IInsumoService service) =>
            {
                await service.AdicionarEstoque(id, dto);
                return Results.NoContent();
            })
            .WithName("AdicionarEstoque")
            .WithSummary("Adiciona quantidade ao estoque de um insumo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/remover-estoque", async (
                Guid id,
                AtualizarEstoqueDTO dto,
                IInsumoService service) =>
            {
                await service.RemoverEstoque(id, dto);
                return Results.NoContent();
            })
            .WithName("RemoverEstoque")
            .WithSummary("Remove quantidade do estoque de um insumo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", async (Guid id, IInsumoService service) =>
            {
                await service.Delete(id);
                return Results.NoContent();
            })
            .WithName("DeleteInsumo")
            .WithSummary("Remove um insumo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
