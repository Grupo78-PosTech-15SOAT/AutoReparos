using AutoReparos.API.Controllers;
using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
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

            group.MapGet("/", (
                OrdemServicoController controller,
                [AsParameters] OrdemServicoPagedRequest request) => controller.GetAll(request))
            .WithName("GetAllOrdensServico")
            .WithSummary("Lista todas as ordens de serviço paginada")
            .Produces<PagedResult<OrdemServicoDto>>(StatusCodes.Status200OK);

            group.MapGet("/fila", (
                OrdemServicoController controller,
                [AsParameters] PagedRequest request) => controller.GetFila(request))
            .WithName("GetFilaOrdensServico")
            .WithSummary("Lista a fila de trabalho da oficina ordenada por prioridade (exclui LOGICAMENTE os status \"Finalizada\" e \"Entregue\")")
            .Produces<PagedResult<OrdemServicoDto>>(StatusCodes.Status200OK);

            group.MapGet("/consulta", (
                OrdemServicoController controller,
                [AsParameters] OrdemServicoConsultaPagedRequest request) => controller.GetByDocumentoOuPlaca(request))
            .WithName("GetOrdensServicoByDocumentoOuPlaca")
            .WithSummary("Pesquisa ordens de serviço por documento ou placa (Público)")
            .Produces<PagedResult<OrdemServicoPublicoDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

            group.MapGet("/consulta/{id:guid}", (Guid id, OrdemServicoController controller) => controller.GetPublicById(id))
            .WithName("GetPublicOrdemServicoById")
            .WithSummary("Busca uma ordem de serviço por ID com detalhes (Público)")
            .Produces<OrdemServicoPublicoDetalheDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous();

            group.MapPost("/", (CriarOrdemServicoDto dto, OrdemServicoController controller) => controller.Create(dto))
            .WithName("CreateOrdemServico")
            .WithSummary("Cria uma nova ordem de serviço")
            .Produces<OrdemServicoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/{id:guid}", (Guid id, OrdemServicoController controller) => controller.GetById(id))
            .WithName("GetOrdemServicoById")
            .WithSummary("Busca uma ordem de serviço por ID com detalhes")
            .Produces<OrdemServicoDetalheDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/{id:guid}/servicos", (
                Guid id,
                AdicionarServicoDto dto,
                OrdemServicoController controller) => controller.AdicionarServico(id, dto))
            .WithName("AdicionarServico")
            .WithSummary("Adiciona um serviço à ordem de serviço")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/{id:guid}/insumos", (Guid id, AdicionarInsumoDto dto, OrdemServicoController controller) => controller.AdicionarInsumo(id, dto))
            .WithName("AdicionarInsumo")
            .WithSummary("Adiciona um insumo à ordem de serviço")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/iniciar-diagnostico", (
                Guid id,
                OrdemServicoController controller) => controller.IniciarDiagnostico(id))
            .WithName("IniciarDiagnostico")
            .WithSummary("Inicia o diagnóstico da ordem de serviço")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/enviar-para-aprovacao", (
                Guid id,
                OrdemServicoController controller) => controller.EnviarParaAprovacao(id))
            .WithName("EnviarParaAprovacao")
            .WithSummary("Envia a ordem de serviço para aprovação do cliente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/aprovar", (
                string token,
                OrdemServicoController controller) => controller.Aprovar(token))
            .WithName("AprovarOrdemServico")
            .WithSummary("Aprova a ordem de serviço")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

            group.MapGet("/recusar", (
                string token,
                OrdemServicoController controller) => controller.Recusar(token))
            .WithName("RecusarOrdemServico")
            .WithSummary("Recusa a ordem de serviço")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

            group.MapPatch("/{id:guid}/servicos/{servicoId:guid}/iniciar", (
                Guid id,
                Guid servicoId,
                OrdemServicoController controller) => controller.IniciarServico(id, servicoId))
            .WithName("IniciarServico")
            .WithSummary("Inicia a execução de um serviço da OS")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/servicos/{servicoId:guid}/concluir", (
                Guid id,
                Guid servicoId,
                OrdemServicoController controller) => controller.ConcluirServico(id, servicoId))
            .WithName("ConcluirServico")
            .WithSummary("Marca um serviço da OS como concluído")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id:guid}/entregar", (
                Guid id,
                OrdemServicoController controller) => controller.Entregar(id))
            .WithName("EntregarOrdemServico")
            .WithSummary("Registra a entrega do veículo ao cliente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
