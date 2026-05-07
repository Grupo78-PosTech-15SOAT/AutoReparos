using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Servicos.Exceptions
{
    public class InvalidServicoException(string message) : DomainException(message);
}
