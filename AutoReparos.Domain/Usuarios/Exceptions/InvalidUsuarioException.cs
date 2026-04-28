using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Usuarios.Exceptions
{
    public sealed class InvalidUsuarioException(string message) : DomainException(message);
}
