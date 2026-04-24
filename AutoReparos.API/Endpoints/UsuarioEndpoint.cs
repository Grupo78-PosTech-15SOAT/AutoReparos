using AutoReparos.Application.Shared;
using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.DTOs.Response;
using AutoReparos.Application.Usuarios.Services.Interfaces;

namespace AutoReparos.API.Endpoints
{
    public static class UsuarioEndpoint
    {
        public static void MapUsuariosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/usuarios")
                .WithTags("Usuários");

            group.MapPost("/", async (UsuarioCreateDTO dto, IUsuarioService service) =>
            {
                var usuario = await service.Create(dto);
                return Results.Created($"/api/usuarios/{usuario.Id}", usuario);
            })
            .WithName("CreateUsuario")
            .WithSummary("Cadastra um novo usuário")
            .WithDescription("Cria um novo usuário no sistema (Administrador, Atendente ou Mecânico)")
            .Produces<UsuarioDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/", async (IUsuarioService service, string? nome, int pageNumber = 1, int pageSize = 10) =>
            {
                var request = new PagedRequest
                {
                    Nome = nome,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await service.GetAll(request);
                return Results.Ok(result);
            })
            .WithName("GetAllUsuarios")
            .WithSummary("Lista todos os usuários")
            .Produces<PagedResult<UsuarioDTO>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", async (Guid id, IUsuarioService service) =>
            {
                var usuario = await service.GetById(id);
                return usuario is null ? Results.NotFound() : Results.Ok(usuario);
            })
            .WithName("GetUsuarioById")
            .WithSummary("Busca um usuário por Id")
            .Produces<UsuarioDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id:guid}", async (Guid id, UsuarioUpdateDTO dto, IUsuarioService service) =>
            {
                await service.Update(id, dto);
                return Results.NoContent();
            })
            .WithName("UpdateUsuario")
            .WithSummary("Atualiza um usuário")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:guid}", async (Guid id, IUsuarioService service) =>
            {
                await service.Delete(id);
                return Results.NoContent();
            })
            .WithName("DeleteUsuario")
            .WithSummary("Exclui um usuário")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
