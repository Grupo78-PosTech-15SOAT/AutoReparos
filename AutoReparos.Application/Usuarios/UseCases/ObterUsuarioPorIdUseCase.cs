using AutoReparos.Application.Usuarios.DTOs.Response;
using AutoReparos.Application.Usuarios.Mappers;
using AutoReparos.Application.Usuarios.UseCases.Interfaces;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Usuarios.Repositories;

namespace AutoReparos.Application.Usuarios.UseCases
{
    public class ObterUsuarioPorIdUseCase(IUsuarioRepository usuarioRepository) : IObterUsuarioPorIdUseCase
    {
        public async Task<UsuarioDto?> ExecuteAsync(Guid id)
        {
            var usuario = await usuarioRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Usuário não encontrado no sistema.");

            return UsuarioMapper.ToDto(usuario);
        }
    }
}
