using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Veiculos.ValueObjects.Exceptions
{
    public sealed class InvalidVeiculoException(string message) : DomainException(message);
}