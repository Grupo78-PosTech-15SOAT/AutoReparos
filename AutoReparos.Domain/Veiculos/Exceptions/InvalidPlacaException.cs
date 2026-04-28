using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Veiculos.Exceptions
{
    public sealed class InvalidPlacaException(string message) : DomainException(message);
}
