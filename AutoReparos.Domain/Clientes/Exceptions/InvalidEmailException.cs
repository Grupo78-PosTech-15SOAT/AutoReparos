using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Clientes.Exceptions
{
    public sealed class InvalidEmailException(string message) : DomainException(message);
}
