using AutoReparos.Domain.Clientes.Exceptions;

namespace AutoReparos.Domain.Clientes.ValueObjects
{
    public sealed class Telefone
    {
        public string Numero { get; }

        private Telefone(string numero)
        {
            Numero = numero;
        }

        public static Telefone Create(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                throw new InvalidTelefoneException("O telefone é obrigatório.");

            numero = new string(numero.Where(char.IsDigit).ToArray());

            if (!TelefoneValido(numero))
                throw new InvalidTelefoneException("Telefone inválido.");

            return new Telefone(numero);
        }

        private static bool TelefoneValido(string telefone)
        {
            // Celular com DDD: 11999999999 (11 dígitos)
            // Fixo com DDD: 1133334444 (10 dígitos)

            if (telefone.Length != 10 && telefone.Length != 11)
                return false;

            // DDD não pode começar com 0
            if (telefone.StartsWith('0'))
                return false;

            return true;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Telefone other)
                return false;

            return Numero == other.Numero;
        }

        public override int GetHashCode()
        {
            return Numero.GetHashCode();
        }

        public static implicit operator string(Telefone telefone) => telefone.ToString();

        public override string ToString() => Numero;
    }
}
