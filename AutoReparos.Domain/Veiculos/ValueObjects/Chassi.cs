using AutoReparos.Domain.Veiculos.Exceptions;

namespace AutoReparos.Domain.Veiculos.ValueObjects
{
    public sealed record Chassi
    {
        public string Valor { get; }

        public Chassi(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new InvalidChassiException("Chassi é obrigatório");

            valor = valor.Trim().ToUpper();

            if (valor.Length != 17)
                throw new InvalidChassiException("Chassi deve ter 17 caracteres");

            Valor = valor;
        }
    }
}
