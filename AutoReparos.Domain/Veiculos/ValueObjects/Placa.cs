using AutoReparos.Domain.Veiculos.Exceptions;
using System.Drawing;
using System.Text.RegularExpressions;

namespace AutoReparos.Domain.Veiculos.ValueObjects
{
    public sealed partial record Placa
    {
        private const string PatternPlacaAntiga = "^[A-Z]{3}[0-9]{4}$";
        private const string PatternPlacaNova = "^[A-Z]{3}[0-9][A-Z][0-9]{2}$";

        public string Valor { get; }

        public Placa(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new InvalidPlacaException("Placa é obrigatória");

            valor = Normalizar(valor);

            if (!PlacaAntigaRegex().IsMatch(valor) && !PlacaNovaRegex().IsMatch(valor))
                throw new InvalidPlacaException("Placa inválida");

            Valor = valor;
        }

        public static string Normalizar(string valor) => valor.ToUpper().Replace("-", "").Trim();

        public override string ToString() => Valor;


        [GeneratedRegex(PatternPlacaAntiga)]
        private static partial Regex PlacaAntigaRegex();
        
        [GeneratedRegex(PatternPlacaNova)]
        private static partial Regex PlacaNovaRegex();
    }
}
