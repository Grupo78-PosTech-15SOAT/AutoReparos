using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Veiculos.ValueObjects.Exceptions
{
    public sealed class InvalidRenavamException(string message) : DomainException(message);
}
