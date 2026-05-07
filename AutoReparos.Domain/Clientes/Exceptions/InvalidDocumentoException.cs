using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Clientes.Exceptions
{
    public sealed class InvalidDocumentoException(string message) : DomainException(message);
}
