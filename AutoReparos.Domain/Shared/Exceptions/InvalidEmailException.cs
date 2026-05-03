using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Shared.Exceptions
{
    public sealed class InvalidEmailException(string message) : DomainException(message);
}