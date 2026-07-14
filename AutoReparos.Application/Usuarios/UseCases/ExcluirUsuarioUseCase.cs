using AutoReparos.Application.Usuarios.UseCases.Interfaces;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Usuarios.Repositories;

namespace AutoReparos.Application.Usuarios.UseCases
{
    public class ExcluirUsuarioUseCase(IUsuarioRepository usuarioRepository) : IExcluirUsuarioUseCase
    {
        public async Task ExecuteAsync(Guid id)
        {
            var usuario = await usuarioRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("O usuário que você está tentando excluir não existe.");

            await usuarioRepository.DeleteAsync(usuario);
        }
    }
}
