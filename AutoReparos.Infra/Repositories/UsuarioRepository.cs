using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Exceptions;
using AutoReparos.Domain.Usuarios.Repositories;
using AutoReparos.Infra.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoReparos.Infra.Repositories
{
    public class UsuarioRepository(UserManager<UsuarioIdentity> userManager) : IUsuarioRepository
    {
        private readonly UserManager<UsuarioIdentity> _userManager = userManager;

        public async Task CreateAsync(Usuario usuario, string password)
        {
            var identityUser = new UsuarioIdentity(
                usuario.Id,
                usuario.NomeCompleto,
                usuario.Email.Endereco,
                usuario.Tipo,
                usuario.CriadoEm
            );

            var result = await _userManager.CreateAsync(identityUser, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new InvalidUsuarioException($"Não foi possível cadastrar o usuário: {errors}");
            }
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            var identityUser = await _userManager.FindByIdAsync(usuario.Id.ToString());
            if (identityUser == null) return;

            identityUser.NomeCompleto = usuario.NomeCompleto;
            identityUser.Tipo = usuario.Tipo;
            identityUser.AtualizadoEm = usuario.AtualizadoEm;

            var result = await _userManager.UpdateAsync(identityUser);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new InvalidUsuarioException($"Erro ao atualizar os dados do usuário: {errors}");
            }
        }

        public async Task DeleteAsync(Usuario usuario)
        {
            var identityUser = await _userManager.FindByIdAsync(usuario.Id.ToString());
            if (identityUser == null) return;

            var result = await _userManager.DeleteAsync(identityUser);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new InvalidUsuarioException($"Não foi possível remover o usuário: {errors}");
            }
        }

        public async Task<Usuario?> GetByIdAsync(Guid id)
        {
            var identityUser = await _userManager.FindByIdAsync(id.ToString());
            return identityUser == null ? null : MapToDomain(identityUser);
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            var identityUser = await _userManager.FindByEmailAsync(email);
            return identityUser == null ? null : MapToDomain(identityUser);
        }

        public async Task<(IEnumerable<Usuario> Items, int TotalCount)> GetAllAsync(string? nome, int skip, int take)
        {
            var query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                query = query.Where(u => u.NomeCompleto.Contains(nome));
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.NomeCompleto)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (users.Select(MapToDomain), total);
        }

        private static Usuario MapToDomain(UsuarioIdentity identityUser)
        {
            return Usuario.Load(
                identityUser.Id,
                identityUser.NomeCompleto,
                identityUser.Email!,
                identityUser.Tipo,
                identityUser.CriadoEm,
                identityUser.AtualizadoEm);
        }
    }
}
