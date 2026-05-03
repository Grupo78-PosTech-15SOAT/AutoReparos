using AutoReparos.Domain.Shared.Exceptions;
using System.Text.RegularExpressions;

namespace AutoReparos.Domain.Shared.ValueObjects
{
    public sealed partial record Email
    {
        private const string Pattern = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        public string Endereco { get; }

        private Email(string endereco)
        {
            Endereco = endereco;
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

        public override string ToString() => Endereco;

        [GeneratedRegex(Pattern)]
        private static partial Regex EmailRegex();
    }
}
