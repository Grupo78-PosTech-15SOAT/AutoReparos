using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.OrdensServicos.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AutoReparos.API.Endpoints
{
    public static class OrdensServicoEndpoints
    {
        public static void MapOrdensServicoEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/ordens-servico")
                .WithTags("OrdensServico")
                .RequireAuthorization();

            group.MapPost("/", async (CriarOrdemServicoDTO dto, IOrdemServicoService service) =>
            {
                var os = await service.Create(dto);
                return Results.CreatedAtRoute("GetOrdemServicoById", new { id = os.Id }, os);
            })
            .WithName("CreateOrdemServico")
            .WithSummary("Cria uma nova ordem de serviço")
            .Produces<OrdemServicoDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/{id:guid}", async (Guid id, IOrdemServicoService service) =>
            {
                var os = await service.GetById(id);
                return os is null ? Results.NotFound() : Results.Ok(os);
            })
            .WithName("GetOrdemServicoById")
            .WithSummary("Busca uma ordem de serviço por ID com detalhes")
            .Produces<OrdemServicoDetalheDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/", async (
                IOrdemServicoService service,
                [FromQuery] Guid? clienteId,
                [FromQuery] Guid? veiculoId,
                [FromQuery] EStatusOrdemServico? status,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var result = await service.GetAll(clienteId, veiculoId, status, pageNumber, pageSize);
                return Results.Ok(result);
            })
            .WithName("GetAllOrdensServico")
            .WithSummary("Lista todas as ordens de serviço paginadas")
            .Produces<PagedResult<OrdemServicoDTO>>(StatusCodes.Status200OK);

            group.MapPost("/{id:guid}/servicos", async (
                Guid id,
                AdicionarServicoDTO dto,
                IOrdemServicoService service) =>
            {
                await service.AdicionarServico(id, dto);
                return Results.NoContent();
            })
            .WithName("AdicionarServico")
            .WithSummary("Adiciona um serviço à ordem de serviço")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/{id:guid}/insumos", async (Guid id, AdicionarInsumoDTO dto, IOrdemServicoService service) =>
            {
                await service.AdicionarInsumo(id, dto);
                return Results.NoContent();
            })
            .WithName("AdicionarInsumo")
            .WithSummary("Adiciona um insumo à ordem de serviço")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/iniciar-diagnostico", async (
                Guid id,
                IOrdemServicoService service) =>
            {
                await service.IniciarDiagnostico(id);
                return Results.NoContent();
            })
            .WithName("IniciarDiagnostico")
            .WithSummary("Inicia o diagnóstico da ordem de serviço")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/enviar-para-aprovacao", async (
                Guid id,
                IOrdemServicoService service) =>
            {
                await service.AguardarAprovacao(id);
                return Results.NoContent();
            })
            .WithName("AguardarAprovacao")
            .WithSummary("Envia a ordem de serviço para aprovação do cliente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/aprovar", async (
                Guid id,
                IOrdemServicoService service) =>
            {
                await service.Aprovar(id);
                return Results.NoContent();
            })
            .WithName("AprovarOrdemServico")
            .WithSummary("Aprova a ordem de serviço")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/servicos/{servicoId:guid}/iniciar", async (
                Guid id,
                Guid servicoId,
                IOrdemServicoService service) =>
            {
                await service.IniciarServicoAsync(id, servicoId);
                return Results.NoContent();
            })
            .WithName("IniciarServico")
            .WithSummary("Inicia a execução de um serviço da OS")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/servicos/{servicoId:guid}/concluir", async (
                Guid id,
                Guid servicoId,
                IOrdemServicoService service) =>
            {
                await service.ConcluirServico(id, servicoId);
                return Results.NoContent();
            })
            .WithName("ConcluirServico")
            .WithSummary("Marca um serviço da OS como concluído")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/entregar", async (
                Guid id,
                IOrdemServicoService service) =>
            {
                await service.Entregar(id);
                return Results.NoContent();
            })
            .WithName("EntregarOrdemServico")
            .WithSummary("Registra a entrega do veículo ao cliente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
