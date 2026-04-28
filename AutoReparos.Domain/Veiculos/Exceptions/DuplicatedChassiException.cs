using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.Veiculos.Exceptions
{
    public sealed class DuplicatedChassiException() : DomainException("Já existe um veículo com esse chassi");
}