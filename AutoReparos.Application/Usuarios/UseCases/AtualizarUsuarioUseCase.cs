using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.UseCases.Interfaces;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Usuarios.Repositories;

namespace AutoReparos.Application.Usuarios.UseCases
{
    public class AtualizarUsuarioUseCase(IUsuarioRepository usuarioRepository) : IAtualizarUsuarioUseCase
    {
        public async Task ExecuteAsync(Guid id, UsuarioUpdateDto dto)
        {
            var usuario = await usuarioRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Usuário não encontrado para atualização.");

            usuario.Atualizar(dto.NomeCompleto, dto.Tipo);

            await usuarioRepository.UpdateAsync(usuario);
        }
    }
}
