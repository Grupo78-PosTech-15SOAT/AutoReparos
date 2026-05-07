namespace AutoReparos.Domain.Shared.Exceptions
{
    public sealed class NotFoundException(string message) : DomainException(message);
}
