using AutoReparos.Domain.Veiculos.Exceptions;
using System.Text.RegularExpressions;

namespace AutoReparos.Domain.Veiculos.ValueObjects
{
    public sealed record Placa
    {
        public string Valor { get; }

        public Placa(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new InvalidPlacaException("Placa é obrigatória");

            valor = Normalizar(valor);

            // Mercosul ou antigo
            var regexAntiga = new Regex(@"^[A-Z]{3}[0-9]{4}$");
            var regexNova = new Regex(@"^[A-Z]{3}[0-9][A-Z][0-9]{2}$");

            if (!regexAntiga.IsMatch(valor) && !regexNova.IsMatch(valor))
                throw new InvalidPlacaException("Placa inválida");

            Valor = valor;
        }

        public static string Normalizar(string valor) => valor.ToUpper().Replace("-", "").Trim();

        public override string ToString() => Valor;
    }
}
