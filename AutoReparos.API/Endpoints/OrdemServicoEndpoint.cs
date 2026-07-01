using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.Shared;

namespace AutoReparos.API.Endpoints
{
    public static class OrdemServicoEndpoint
    {
        public static void MapOrdemServicoEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/ordem-servico")
                .WithTags("OrdensServico")
                .RequireAuthorization();

            group.MapGet("/", async (
                IOrdemServicoService service,
                [AsParameters] OrdemServicoPagedRequest request) =>
            {
                var result = await service.GetAll(request);
                return Results.Ok(result);
            })
            .WithName("GetAllOrdensServico")
            .WithSummary("Lista todas as ordens de serviço paginada")
            .Produces<PagedResult<OrdemServicoDto>>(StatusCodes.Status200OK);

            group.MapGet("/fila", async (
                IOrdemServicoService service,
                [AsParameters] PagedRequest request) =>
            {
                var result = await service.GetFila(request);
                return Results.Ok(result);
            })
            .WithName("GetFilaOrdensServico")
            .WithSummary("Lista a fila de trabalho da oficina ordenada por prioridade (exclui LOGICAMENTE os status \"Finalizada\" e \"Entregue\")")
            .Produces<PagedResult<OrdemServicoDto>>(StatusCodes.Status200OK);

            group.MapGet("/consulta", async (
                IOrdemServicoService service,
                [AsParameters] OrdemServicoConsultaPagedRequest request) =>
            {
                var result = await service.GetByDocumentoOuPlaca(request);
                return Results.Ok(result);
            })
            .WithName("GetOrdensServicoByDocumentoOuPlaca")
            .WithSummary("Pesquisa ordens de serviço por documento ou placa (Público)")
            .Produces<PagedResult<OrdemServicoPublicoDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

            group.MapGet("/consulta/{id:guid}", async (Guid id, IOrdemServicoService service) =>
            {
                var os = await service.GetPublicById(id);
                return os is null ? Results.NotFound() : Results.Ok(os);
            })
            .WithName("GetPublicOrdemServicoById")
            .WithSummary("Busca uma ordem de serviço por ID com detalhes (Público)")
            .Produces<OrdemServicoPublicoDetalheDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous();

            group.MapPost("/", async (CriarOrdemServicoDto dto, IOrdemServicoService service) =>
            {
                var os = await service.Create(dto);
                return Results.CreatedAtRoute("GetOrdemServicoById", new { id = os.Id }, os);
            })
            .WithName("CreateOrdemServico")
            .WithSummary("Cria uma nova ordem de serviço")
            .Produces<OrdemServicoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/{id:guid}", async (Guid id, IOrdemServicoService service) =>
            {
                var os = await service.GetById(id);
                return os is null ? Results.NotFound() : Results.Ok(os);
            })
            .WithName("GetOrdemServicoById")
            .WithSummary("Busca uma ordem de serviço por ID com detalhes")
            .Produces<OrdemServicoDetalheDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/{id:guid}/servicos", async (
                Guid id,
                AdicionarServicoDto dto,
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

            group.MapPost("/{id:guid}/insumos", async (Guid id, AdicionarInsumoDto dto, IOrdemServicoService service) =>
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
            .WithName("EnviarParaAprovacao")
            .WithSummary("Envia a ordem de serviço para aprovação do cliente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/aprovar", async (
                string token,
                IOrdemServicoService service) =>
            {
                await service.Aprovar(token);
                return Results.Content("Orçamento aprovado");
            })
            .WithName("AprovarOrdemServico")
            .WithSummary("Aprova a ordem de serviço")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

            group.MapGet("/recusar", async (
                string token,
                IOrdemServicoService service) =>
            {
                await service.Recusar(token);
                return Results.Content("Orçamento recusado");
            })
            .WithName("RecusarOrdemServico")
            .WithSummary("Recusa a ordem de serviço")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

            group.MapPatch("/{id:guid}/servicos/{servicoId:guid}/iniciar", async (
                Guid id,
                Guid servicoId,
                IOrdemServicoService service) =>
            {
                await service.IniciarServico(id, servicoId);
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
