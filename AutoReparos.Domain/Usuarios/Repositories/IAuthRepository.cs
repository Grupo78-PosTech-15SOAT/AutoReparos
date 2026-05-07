using AutoReparos.Domain.Usuarios.Entities;

namespace AutoReparos.Domain.Usuarios.Repositories
{
    public interface IAuthRepository
    {
        /// <summary>
        /// Valida as credenciais de um usuário e o retorna se forem válidas
        /// </summary>
        /// <param name="email">E-mail do usuário</param>
        /// <param name="password">Senha para verificação</param>
        /// <returns>O usuário autenticado ou null se as credenciais forem inválidas</returns>
        Task<Usuario?> ValidateCredentialsAsync(string email, string password);
    }
}
