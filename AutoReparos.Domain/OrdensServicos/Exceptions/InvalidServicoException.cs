using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.OrdensServicos.Exceptions
{
    public class InvalidOrdemServicoException(string message) : DomainException(message);
}
