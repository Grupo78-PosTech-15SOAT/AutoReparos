using AutoReparos.Application.Pecas.DTOs.Request;
using AutoReparos.Application.Pecas.DTOs.Response;
using AutoReparos.Application.Pecas.Services.Interfaces;
using AutoReparos.Application.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AutoReparos.API.Endpoints
{
    public static class PecasEndpoints
    {
        public static void MapPecasEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/pecas")
                .WithTags("Pecas")
                .RequireAuthorization();

            group.MapPost("/", async (CriarPecaDTO dto, IPecaService service) =>
            {
                var peca = await service.Create(dto);
                return Results.CreatedAtRoute("GetPecaById", new { id = peca.Id }, peca);
            })
            .WithName("CreatePeca")
            .WithSummary("Cadastra uma nova peça")
            .Produces<PecaDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/{id:guid}", async (Guid id, IPecaService service) =>
            {
                var peca = await service.GetById(id);
                return peca is null ? Results.NotFound() : Results.Ok(peca);
            })
            .WithName("GetPecaById")
            .WithSummary("Busca uma peça por ID")
            .Produces<PecaDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/", async (
                IPecaService service,
                [FromQuery] string? nome,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var result = await service.GetAll(nome, pageNumber, pageSize);
                return Results.Ok(result);
            })
            .WithName("GetAllPecas")
            .WithSummary("Lista todas as peças paginadas")
            .Produces<PagedResult<PecaDTO>>(StatusCodes.Status200OK);

            group.MapPut("/{id:guid}", async (Guid id, AtualizarPecaDTO dto, IPecaService service) =>
            {
                await service.Update(id, dto);
                return Results.NoContent();
            })
            .WithName("UpdatePeca")
            .WithSummary("Atualiza os dados de uma peça")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/adicionar-estoque", async (
                Guid id,
                AtualizarEstoqueDTO dto,
                IPecaService service) =>
            {
                await service.AdicionarEstoque(id, dto);
                return Results.NoContent();
            })
            .WithName("AdicionarEstoque")
            .WithSummary("Adiciona quantidade ao estoque de uma peça")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/remover-estoque", async (
                Guid id,
                AtualizarEstoqueDTO dto,
                IPecaService service) =>
            {
                await service.RemoverEstoque(id, dto);
                return Results.NoContent();
            })
            .WithName("RemoverEstoque")
            .WithSummary("Remove quantidade do estoque de uma peça")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", async (Guid id, IPecaService service) =>
            {
                await service.Delete(id);
                return Results.NoContent();
            })
            .WithName("DeletePeca")
            .WithSummary("Remove uma peça")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
