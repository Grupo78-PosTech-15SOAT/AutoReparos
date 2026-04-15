using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Clientes.ValueObjects.Exceptions
{
    public sealed class InvalidDocumentoException(string message) : DomainException(message);
}
