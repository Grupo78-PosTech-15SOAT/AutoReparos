using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.DTOs.Response;
using AutoReparos.Application.Usuarios.Mappers;
using AutoReparos.Application.Usuarios.UseCases.Interfaces;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Repositories;

namespace AutoReparos.Application.Usuarios.UseCases
{
    public class CriarUsuarioUseCase(IUsuarioRepository usuarioRepository) : ICriarUsuarioUseCase
    {
        public async Task<UsuarioDto> ExecuteAsync(UsuarioCreateDto dto)
        {
            var usuario = new Usuario(dto.NomeCompleto, dto.Email, dto.Tipo);

            await usuarioRepository.CreateAsync(usuario, dto.Password);

            return UsuarioMapper.ToDto(usuario);
        }
    }
}
