namespace AutoReparos.Domain.Veiculos.ValueObjects
{
    public sealed record Renavam
    {
        public string Valor { get; }

        public Renavam(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new Exception("Renavam é obrigatório");

            var digits = new string(valor.Where(char.IsDigit).ToArray());

            if (digits.Length != 11)
                throw new Exception("Renavam inválido");

            Valor = digits;
        }
    }
}