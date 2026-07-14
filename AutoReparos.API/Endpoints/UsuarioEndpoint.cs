using AutoReparos.API.Controllers;
using AutoReparos.Application.Shared;
using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.DTOs.Response;

namespace AutoReparos.API.Endpoints
{
    public static class UsuarioEndpoint
    {
        public static void MapUsuariosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/usuarios")
                .WithTags("Usuários")
                .RequireAuthorization();

            group.MapPost("/", (UsuarioCreateDto dto, UsuarioController controller) => controller.Create(dto))
            .WithName("CreateUsuario")
            .WithSummary("Cadastra um novo usuário")
            .WithDescription("Cria um novo usuário no sistema (Administrador, Atendente ou Mecânico)")
            .Produces<UsuarioDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/", (UsuarioController controller, [AsParameters] UsuarioPagedRequest request) => controller.GetAll(request))
            .WithName("GetAllUsuarios")
            .WithSummary("Lista todos os usuários")
            .Produces<PagedResult<UsuarioDto>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", (Guid id, UsuarioController controller) => controller.GetById(id))
            .WithName("GetUsuarioById")
            .WithSummary("Busca um usuário por Id")
            .Produces<UsuarioDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id:guid}", (Guid id, UsuarioUpdateDto dto, UsuarioController controller) => controller.Update(id, dto))
            .WithName("UpdateUsuario")
            .WithSummary("Atualiza um usuário")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", (Guid id, UsuarioController controller) => controller.Delete(id))
            .WithName("DeleteUsuario")
            .WithSummary("Exclui um usuário")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
