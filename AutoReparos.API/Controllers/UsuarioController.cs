using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.UseCases.Interfaces;

namespace AutoReparos.API.Controllers
{
    public class UsuarioController(
        ICriarUsuarioUseCase criarUsuarioUseCase,
        IObterUsuarioPorIdUseCase obterUsuarioPorIdUseCase,
        IListarUsuariosUseCase listarUsuariosUseCase,
        IAtualizarUsuarioUseCase atualizarUsuarioUseCase,
        IExcluirUsuarioUseCase excluirUsuarioUseCase)
    {
        public async Task<IResult> Create(UsuarioCreateDto dto)
        {
            var usuario = await criarUsuarioUseCase.ExecuteAsync(dto);
            return Results.Created($"/api/usuarios/{usuario.Id}", usuario);
        }

        public async Task<IResult> GetAll(UsuarioPagedRequest request)
        {
            var result = await listarUsuariosUseCase.ExecuteAsync(request);
            return Results.Ok(result);
        }

        public async Task<IResult> GetById(Guid id)
        {
            var usuario = await obterUsuarioPorIdUseCase.ExecuteAsync(id);
            return usuario is null ? Results.NotFound() : Results.Ok(usuario);
        }

        public async Task<IResult> Update(Guid id, UsuarioUpdateDto dto)
        {
            await atualizarUsuarioUseCase.ExecuteAsync(id, dto);
            return Results.NoContent();
        }

        public async Task<IResult> Delete(Guid id)
        {
            await excluirUsuarioUseCase.ExecuteAsync(id);
            return Results.NoContent();
        }
    }
}
