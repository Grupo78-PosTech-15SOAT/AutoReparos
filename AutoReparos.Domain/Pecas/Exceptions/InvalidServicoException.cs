using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Pecas.Exceptions
{
    public class InvalidPecaException(string message) : DomainException(message);
}
