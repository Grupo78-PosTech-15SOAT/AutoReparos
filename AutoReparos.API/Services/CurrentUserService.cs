using AutoReparos.Application.Shared.Interfaces;
using System.Security.Claims;

namespace AutoReparos.API.Services
{
    /// <summary>
    /// Implementação de ICurrentUserService que extrai o ID do usuário autenticado
    /// a partir do contexto HTTP e do token JWT.
    /// </summary>
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        public string GetUserId()
        {
            var userId = httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("Usuário não autenticado ou token inválido.");

            return userId;
        }
    }
}
