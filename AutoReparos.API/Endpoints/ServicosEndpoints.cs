using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Servicos.Services.Interfaces;
using AutoReparos.Application.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AutoReparos.API.Endpoints
{
    public static class ServicosEndpoints
    {
        public static void MapServicosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/servicos")
                .WithTags("Servicos")
                .RequireAuthorization();

            group.MapPost("/", async (CriarServicoDTO dto, IServicoService service) =>
            {
                var servico = await service.Create(dto);
                return Results.CreatedAtRoute("GetServicoById", new { id = servico.Id }, servico);
            })
            .WithName("CreateServico")
            .WithSummary("Cadastra um novo serviço")
            .Produces<ServicoDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/{id:guid}", async (Guid id, IServicoService service) =>
            {
                var servico = await service.GetById(id);
                return servico is null ? Results.NotFound() : Results.Ok(servico);
            })
            .WithName("GetServicoById")
            .WithSummary("Busca um serviço por ID")
            .Produces<ServicoDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/", async (IServicoService service, [FromQuery] string? nome, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10) =>
            {
                var result = await service.GetAll(nome, pageNumber, pageSize);
                return Results.Ok(result);
            })
            .WithName("GetAllServicos")
            .WithSummary("Lista todos os serviços paginados")
            .Produces<PagedResult<ServicoDTO>>(StatusCodes.Status200OK);

            group.MapGet("/tempo-medio", async (IServicoService service) =>
            {
                var result = await service.GetTempoMedio();
                return Results.Ok(result);
            })
            .WithName("GetTempoMedioServicos")
            .WithSummary("Lista o tempo médio de execução de todos os serviços")
            .Produces<IEnumerable<TempoMedioServicoDTO>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}/tempo-medio", async (Guid id, IServicoService service) =>
            {
                var result = await service.GetTempoMedioById(id);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("GetTempoMedioServicoById")
            .WithSummary("Busca o tempo médio de execução de um serviço específico")
            .Produces<TempoMedioServicoDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id:guid}", async (Guid id, AtualizarServicoDTO dto, IServicoService service) =>
            {
                await service.Update(id, dto);
                return Results.NoContent();
            })
            .WithName("UpdateServico")
            .WithSummary("Atualiza os dados de um serviço")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", async (Guid id, IServicoService service) =>
            {
                await service.Delete(id);
                return Results.NoContent();
            })
            .WithName("DeleteServico")
            .WithSummary("Remove um serviço")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
