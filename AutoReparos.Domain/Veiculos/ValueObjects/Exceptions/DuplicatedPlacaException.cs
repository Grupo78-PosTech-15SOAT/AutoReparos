using AutoReparos.Domain.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Domain.Veiculos.ValueObjects.Exceptions
{
    public sealed class DuplicatedPlacaException() : DomainException("Já existe um veículo com essa placa");
}
