using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Clientes.ValueObjects.Exceptions
{
    public sealed class InvalidEmailException(string message) : DomainException(message);
}
