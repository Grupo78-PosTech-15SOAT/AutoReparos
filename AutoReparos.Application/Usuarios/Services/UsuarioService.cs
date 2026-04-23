using AutoReparos.Application.Shared;
using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.DTOs.Response;
using AutoReparos.Application.Usuarios.Services.Interfaces;
using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoReparos.Application.Usuarios.Services
{
    public class UsuarioService(UserManager<Usuario> userManager) : IUsuarioService
    {
        private readonly UserManager<Usuario> _userManager = userManager;

        public async Task<UsuarioDTO> Create(UsuarioCreateDTO dto)
        {
            var email = Email.Create(dto.Email);

            var usuario = new Usuario(dto.NomeCompleto, email, dto.Tipo);

            var result = await _userManager.CreateAsync(usuario, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new InvalidUsuarioException($"Não foi possível cadastrar o usuário: {errors}");
            }

            return ToDTO(usuario);
        }

        public async Task<UsuarioDTO?> GetById(Guid id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("Usuário não encontrado no sistema.");

            return ToDTO(usuario);
        }

        public async Task<PagedResult<UsuarioDTO>> GetAll(PagedRequest request)
        {
            var query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Nome))
            {
                query = query.Where(u => u.NomeCompleto.Contains(request.Nome));
            }

            var total = await query.CountAsync();
            var usuarios = await query
                .OrderBy(u => u.NomeCompleto)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var items = usuarios.Select(ToDTO);
            return new PagedResult<UsuarioDTO>(items, total, request.PageNumber, request.PageSize);
        }

        public async Task Update(Guid id, UsuarioUpdateDTO dto)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("Usuário não encontrado para atualização.");

            usuario.Atualizar(dto.NomeCompleto, dto.Tipo);

            var result = await _userManager.UpdateAsync(usuario);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new InvalidUsuarioException($"Erro ao atualizar os dados do usuário: {errors}");
            }
        }

        public async Task Delete(Guid id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("O usuário que você está tentando excluir não existe.");

            var result = await _userManager.DeleteAsync(usuario);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new InvalidUsuarioException($"Não foi possível remover o usuário: {errors}");
            }
        }

        private static UsuarioDTO ToDTO(Usuario u) => new(
            u.Id,
            u.NomeCompleto,
            u.Email!,
            u.Tipo,
            u.CriadoEm
        );
    }
}
