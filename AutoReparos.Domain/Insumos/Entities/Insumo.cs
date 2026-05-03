using AutoReparos.Domain.Insumos.Exceptions;
using AutoReparos.Domain.Shared;

namespace AutoReparos.Domain.Insumos.Entities
{
    public class Insumo : Entity
    {
        public string Nome { get; private set; } = null!;
        public string? Descricao { get; private set; }
        public decimal Valor { get; private set; }
        public int QuantidadeEstoque { get; private set; }
        public DateTime CriadoEm { get; }
        public DateTime? AtualizadoEm { get; private set; }

        protected Insumo() { }

        public Insumo(string nome, string? descricao, decimal valor, int quantidadeEstoque) : base()
        {
            Validar(nome, valor, quantidadeEstoque);

            Nome = nome;
            Descricao = descricao;
            Valor = valor;
            QuantidadeEstoque = quantidadeEstoque;
            CriadoEm = DateTime.UtcNow;
        }

        public void Atualizar(string nome, string? descricao, decimal valor)
        {
            Validar(nome, valor, QuantidadeEstoque);

            Nome = nome;
            Descricao = descricao;
            Valor = valor;
            AtualizadoEm = DateTime.UtcNow;
        }

        public void AdicionarEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new InvalidInsumoException("Quantidade deve ser maior que zero.");

            QuantidadeEstoque += quantidade;
            AtualizadoEm = DateTime.UtcNow;
        }

        public void RemoverEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new InvalidInsumoException("Quantidade deve ser maior que zero.");

            if (quantidade > QuantidadeEstoque)
                throw new InvalidInsumoException("Quantidade insuficiente em estoque.");

            QuantidadeEstoque -= quantidade;
            AtualizadoEm = DateTime.UtcNow;
        }

        private static void Validar(string nome, decimal valor, int quantidadeEstoque)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new InvalidInsumoException("Nome é obrigatório.");

            if (nome.Length > 100)
                throw new InvalidInsumoException("Nome deve ter no máximo 100 caracteres.");

            if (valor <= 0)
                throw new InvalidInsumoException("Valor deve ser maior que zero.");

            if (quantidadeEstoque < 0)
                throw new InvalidInsumoException("Quantidade em estoque não pode ser negativa.");
        }
    }
}
