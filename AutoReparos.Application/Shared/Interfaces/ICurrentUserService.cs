namespace AutoReparos.Application.Shared.Interfaces
{
    /// <summary>
    /// Abstração para obter o usuário autenticado da requisição atual.
    /// Implementado na camada de API, injetado nos use cases da Application.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Retorna o identificador do usuário autenticado extraído do JWT (claim NameIdentifier).
        /// Lança exceção se não houver usuário autenticado.
        /// </summary>
        string GetUserId();
    }
}
