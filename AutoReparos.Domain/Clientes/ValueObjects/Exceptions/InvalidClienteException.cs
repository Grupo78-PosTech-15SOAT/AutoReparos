using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Clientes.ValueObjects.Exceptions
{
    public sealed class InvalidClienteException(string message) : DomainException(message);
}