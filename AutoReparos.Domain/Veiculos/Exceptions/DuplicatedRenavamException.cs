using AutoReparos.Domain.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Domain.Veiculos.Exceptions
{
    public sealed class DuplicatedRenavamException() : DomainException("Já existe um veículo com esse RENAVAM");
}
