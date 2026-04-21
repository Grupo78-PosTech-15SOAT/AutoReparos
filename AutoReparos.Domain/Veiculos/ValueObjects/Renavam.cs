using AutoReparos.Domain.Veiculos.Exceptions;

namespace AutoReparos.Domain.Veiculos.ValueObjects
{
    public sealed record Renavam
    {
        public string Valor { get; }

        public Renavam(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new InvalidRenavamException("Renavam é obrigatório");

            var digits = new string(valor.Where(char.IsDigit).ToArray());

            if (digits.Length != 11)
                throw new InvalidRenavamException("Renavam inválido");

            Valor = digits;
        }
    }
}