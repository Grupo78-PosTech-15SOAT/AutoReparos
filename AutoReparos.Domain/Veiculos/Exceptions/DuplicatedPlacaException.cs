using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Veiculos.Exceptions
{
    public sealed class DuplicatedPlacaException() : DomainException("Já existe um veículo com essa placa");
}
