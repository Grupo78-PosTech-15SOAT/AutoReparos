using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Veiculos.Exceptions
{
    public sealed class InvalidChassiException(string message) : DomainException(message);
}
