using AutoReparos.Domain.Usuarios.Entities;

namespace AutoReparos.Domain.Usuarios.Repositories
{
    public interface IUsuarioRepository
    {
        Task CreateAsync(Usuario usuario, string password);
        Task UpdateAsync(Usuario usuario);
        Task DeleteAsync(Usuario usuario);
        Task<Usuario?> GetByIdAsync(Guid id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<(IEnumerable<Usuario> Items, int TotalCount)> GetAllAsync(string? nome, int skip, int take);
    }
}
