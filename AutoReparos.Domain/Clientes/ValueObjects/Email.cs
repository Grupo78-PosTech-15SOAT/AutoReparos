using AutoReparos.Domain.Clientes.ValueObjects.Exceptions;
using System.Text.RegularExpressions;

namespace AutoReparos.Domain.Clientes.ValueObjects
{
    public sealed partial record Email
    {
        private const string Pattern = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        public string Address { get; }

        private Email(string address)
        {
            Address = address;
        }

        public static Email Create(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new InvalidEmailException("E-mail é obrigatório.");
            }

            address = address.Trim();
            address = address.ToLower();

            if (!EmailRegex().IsMatch(address))
            {
                throw new InvalidEmailException("Endereço de E-mail inválido.");
            }

            return new Email(address);
        }

        public static implicit operator string(Email email) => email.ToString();

        public override string ToString() => Address;

        [GeneratedRegex(Pattern)]
        private static partial Regex EmailRegex();
    }
}
