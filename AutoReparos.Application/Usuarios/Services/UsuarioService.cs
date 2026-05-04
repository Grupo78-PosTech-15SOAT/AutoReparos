using AutoReparos.Application.Shared;
using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.DTOs.Response;
using AutoReparos.Application.Usuarios.Services.Interfaces;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Repositories;

namespace AutoReparos.Application.Usuarios.Services
{
    public class UsuarioService(IUsuarioRepository usuarioRepository) : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

        public async Task<UsuarioDTO> Create(UsuarioCreateDTO dto)
        {
            var usuario = new Usuario(dto.NomeCompleto, dto.Email, dto.Tipo);

            await _usuarioRepository.CreateAsync(usuario, dto.Password);

            return ToDTO(usuario);
        }

        public async Task<UsuarioDTO?> GetById(Guid id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id) ?? throw new NotFoundException("Usuário não encontrado no sistema.");

            return ToDTO(usuario);
        }

        public async Task<PagedResult<UsuarioDTO>> GetAll(UsuarioPagedRequest request)
        {
            var (usuarios, total) = await _usuarioRepository.GetAllAsync(
                request.Nome,
                request.Skip,
                request.PageSize);

            var items = usuarios.Select(ToDTO);
            return new PagedResult<UsuarioDTO>(items, total, request.PageNumber, request.PageSize);
        }

        public async Task Update(Guid id, UsuarioUpdateDTO dto)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id) ?? throw new NotFoundException("Usuário não encontrado para atualização.");

            usuario.Atualizar(dto.NomeCompleto, dto.Tipo);

            await _usuarioRepository.UpdateAsync(usuario);
        }

        public async Task Delete(Guid id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id) ?? throw new NotFoundException("O usuário que você está tentando excluir não existe.");

            await _usuarioRepository.DeleteAsync(usuario);
        }

        private static UsuarioDTO ToDTO(Usuario u) => new(
            u.Id,
            u.NomeCompleto,
            u.Email.Endereco,
            u.Tipo,
            u.CriadoEm
        );
    }
}
