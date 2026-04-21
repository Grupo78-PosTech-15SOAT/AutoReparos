using AutoReparos.Domain.Clientes.Enums;
using AutoReparos.Domain.Clientes.Exceptions;

namespace AutoReparos.Domain.Clientes.ValueObjects
{
    public sealed record Documento
    {
        public string Valor { get; }
        public ETipoDocumento Tipo { get; }

        public Documento(string valor)
        {
            if (string.IsNullOrEmpty(valor) || string.IsNullOrWhiteSpace(valor))
                throw new InvalidDocumentoException("O CPF ou CNPJ é obrigatório.");
            var digits = new string(valor.Where(char.IsDigit).ToArray());

            if (digits.Length == 11 && ValidarCpf(digits))
            {
                Valor = digits;
                Tipo = ETipoDocumento.CPF;
            }
            else if (digits.Length == 14 && ValidarCnpj(digits))
            {
                Valor = digits;
                Tipo = ETipoDocumento.CNPJ;
            }
            else
            {
                throw new InvalidDocumentoException("CPF ou CNPJ inválido.");
            }
        }

        #region Validação

        private static bool ValidarCpf(string digits)
        {
            // rejeita sequências iguais ex: 111.111.111-11
            if (digits.Distinct().Count() == 1) return false;

            var soma = 0;
            for (var i = 0; i < 9; i++)
                soma += int.Parse(digits[i].ToString()) * (10 - i);

            var primeiro = (soma * 10 % 11) == 10 ? 0 : (soma * 10 % 11);
            if (primeiro != int.Parse(digits[9].ToString())) return false;

            soma = 0;
            for (var i = 0; i < 10; i++)
                soma += int.Parse(digits[i].ToString()) * (11 - i);

            var segundo = (soma * 10 % 11) == 10 ? 0 : (soma * 10 % 11);
            return segundo == int.Parse(digits[10].ToString());
        }

        private static bool ValidarCnpj(string digits)
        {
            if (digits.Distinct().Count() == 1) return false;

            int[] multiplicadores1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicadores2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var soma = 0;
            for (var i = 0; i < 12; i++)
                soma += int.Parse(digits[i].ToString()) * multiplicadores1[i];

            var primeiro = soma % 11 < 2 ? 0 : 11 - (soma % 11);
            if (primeiro != int.Parse(digits[12].ToString())) return false;

            soma = 0;
            for (var i = 0; i < 13; i++)
                soma += int.Parse(digits[i].ToString()) * multiplicadores2[i];

            var segundo = soma % 11 < 2 ? 0 : 11 - (soma % 11);
            return segundo == int.Parse(digits[13].ToString());
        }

        #endregion

        public string Formatado => Tipo == ETipoDocumento.CPF
            ? $"{Valor[..3]}.{Valor[3..6]}.{Valor[6..9]}-{Valor[9..11]}"
            : $"{Valor[..2]}.{Valor[2..5]}.{Valor[5..8]}/{Valor[8..12]}-{Valor[12..14]}";

        public override string ToString() => Formatado;

        public static string Normalizar(string valor) => new string(valor.Where(char.IsDigit).ToArray());
    }
}
