using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Servicos.exceptions
{
    public class InvalidServicoException(string message) : DomainException(message);
}
