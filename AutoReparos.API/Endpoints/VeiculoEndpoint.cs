using AutoReparos.API.Controllers;
using AutoReparos.Application.Shared;
using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;

namespace AutoReparos.API.Endpoints
{
    public static class VeiculoEndpoint
    {
        public static void MapVeiculosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/veiculos")
                .WithTags("Veiculos")
                .RequireAuthorization();

            group.MapPost("/", (VeiculoCreateDto dto, VeiculoController controller) => controller.Create(dto))
            .WithName("CreateVeiculo")
            .WithSummary("Cadastra um novo veículo")
            .WithDescription("Endpoint responsável por cadastrar um novo veículo no sistema")
            .Produces<VeiculoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/", (VeiculoController controller, [AsParameters] VeiculoPagedRequest request) => controller.GetAll(request))
            .WithName("GetAllVeiculos")
            .WithSummary("Lista todos os veículos paginados")
            .Produces<PagedResult<VeiculoDto>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", (Guid id, VeiculoController controller) => controller.GetById(id))
            .WithName("GetVeiculoById")
            .WithSummary("Busca veículo por Id")
            .WithDescription("Endpoint responsável por retornar um veículo pelo seu id")
            .Produces<VeiculoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/placa/{placa}", (string placa, VeiculoController controller) => controller.GetByPlaca(placa))
            .WithName("GetVeiculoByPlaca")
            .WithSummary("Busca veículo por placa")
            .WithDescription("Endpoint responsável por retornar um veículo pela sua placa")
            .Produces<VeiculoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id:guid}", (Guid id, VeiculoUpdateDto dto, VeiculoController controller) => controller.Update(id, dto))
            .WithName("UpdateVeiculo")
            .WithSummary("Atualiza os dados de um veículo")
            .WithDescription("Endpoint responsável por atualizar os dados de um veículo pelo seu id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", (Guid id, VeiculoController controller) => controller.Delete(id))
            .WithName("DeleteVeiculo")
            .WithSummary("Exclui um veículo")
            .WithDescription("Endpoint responsável por excluir um veículo pelo seu id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
