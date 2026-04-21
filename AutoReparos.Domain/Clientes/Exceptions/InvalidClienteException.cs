using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Clientes.Exceptions
{
    public sealed class InvalidClienteException(string message) : DomainException(message);
}