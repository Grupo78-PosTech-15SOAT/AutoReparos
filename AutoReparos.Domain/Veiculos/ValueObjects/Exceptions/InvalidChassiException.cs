using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Veiculos.ValueObjects.Exceptions
{
    public sealed class InvalidChassiException(string message) : DomainException(message);
}
