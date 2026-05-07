using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Insumos.Exceptions
{
    public class InvalidInsumoException(string message) : DomainException(message);
}
