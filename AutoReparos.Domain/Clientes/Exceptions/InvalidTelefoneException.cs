using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Clientes.Exceptions
{
    public sealed class InvalidTelefoneException(string message) : DomainException(message);
}
