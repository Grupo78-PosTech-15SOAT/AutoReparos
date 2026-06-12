using AutoReparos.Application.Shared;
using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.DTOs.Response;
using AutoReparos.Application.Veiculos.Services.Interfaces;

namespace AutoReparos.API.Endpoints
{
    public static class VeiculoEndpoint
    {
        public static void MapVeiculosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/veiculos")
                .WithTags("Veiculos")
                .RequireAuthorization();

            group.MapPost("/", async (VeiculoCreateDto dto, IVeiculoService service) =>
            {
                var veiculo = await service.Create(dto);

                return Results.CreatedAtRoute("GetVeiculoById", new { id = veiculo.Id }, veiculo);
            })
            .WithName("CreateVeiculo")
            .WithSummary("Cadastra um novo veículo")
            .WithDescription("Endpoint responsável por cadastrar um novo veículo no sistema")
            .Produces<VeiculoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/", async (
                IVeiculoService service,
                [AsParameters] VeiculoPagedRequest request) =>
            {
                var result = await service.GetAll(request);
                return Results.Ok(result);
            })
            .WithName("GetAllVeiculos")
            .WithSummary("Lista todos os veículos paginados")
            .Produces<PagedResult<VeiculoDto>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", async (Guid id, IVeiculoService service) =>
            {
                var veiculo = await service.GetById(id);

                return veiculo is null
                    ? Results.NotFound()
                    : Results.Ok(veiculo);
            })
            .WithName("GetVeiculoById")
            .WithSummary("Busca veículo por Id")
            .WithDescription("Endpoint responsável por retornar um veículo pelo seu id")
            .Produces<VeiculoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/placa/{placa}", async (string placa, IVeiculoService service) =>
            {
                var veiculo = await service.GetByPlaca(placa);
                return veiculo is null
                    ? Results.NotFound()
                    : Results.Ok(veiculo);
            })
            .WithName("GetVeiculoByPlaca")
            .WithSummary("Busca veículo por placa")
            .WithDescription("Endpoint responsável por retornar um veículo pela sua placa")
            .Produces<VeiculoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id:guid}", async (Guid id, VeiculoUpdateDto dto, IVeiculoService service) =>
            {
                await service.Update(id, dto);

                return Results.NoContent();
            })
            .WithName("UpdateVeiculo")
            .WithSummary("Atualiza os dados de um veículo")
            .WithDescription("Endpoint responsável por atualizar os dados de um veículo pelo seu id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", async (Guid id, IVeiculoService service) =>
            {
                await service.Delete(id);

                return Results.NoContent();
            })
            .WithName("DeleteVeiculo")
            .WithSummary("Exclui um veículo")
            .WithDescription("Endpoint responsável por excluir um veículo pelo seu id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}