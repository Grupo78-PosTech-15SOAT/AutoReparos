using AutoReparos.API.Controllers;
using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Shared;

namespace AutoReparos.API.Endpoints
{
    public static class ServicosEndpoints
    {
        public static void MapServicosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/servicos")
                .WithTags("Servicos")
                .RequireAuthorization();

            group.MapPost("/", (CriarServicoDto dto, ServicoController controller) => controller.Create(dto))
            .WithName("CreateServico")
            .WithSummary("Cadastra um novo serviço")
            .Produces<ServicoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/{id:guid}", (Guid id, ServicoController controller) => controller.GetById(id))
            .WithName("GetServicoById")
            .WithSummary("Busca um serviço por ID")
            .Produces<ServicoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/", (ServicoController controller, [AsParameters] ServicoPagedRequest request) => controller.GetAll(request))
            .WithName("GetAllServicos")
            .WithSummary("Lista todos os serviços paginados")
            .Produces<PagedResult<ServicoDto>>(StatusCodes.Status200OK);

            group.MapGet("/tempo-medio", (ServicoController controller) => controller.GetTempoMedio())
            .WithName("GetTempoMedioServicos")
            .WithSummary("Lista o tempo médio de execução de todos os serviços")
            .Produces<IEnumerable<TempoMedioServicoDto>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}/tempo-medio", (Guid id, ServicoController controller) => controller.GetTempoMedioById(id))
            .WithName("GetTempoMedioServicoById")
            .WithSummary("Busca o tempo médio de execução de um serviço específico")
            .Produces<TempoMedioServicoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id:guid}", (Guid id, AtualizarServicoDto dto, ServicoController controller) => controller.Update(id, dto))
            .WithName("UpdateServico")
            .WithSummary("Atualiza os dados de um serviço")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", (Guid id, ServicoController controller) => controller.Delete(id))
            .WithName("DeleteServico")
            .WithSummary("Remove um serviço")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
