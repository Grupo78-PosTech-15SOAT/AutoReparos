using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Veiculos.Exceptions
{
    public sealed class DuplicatedRenavamException() : DomainException("Já existe um veículo com esse RENAVAM");
}
