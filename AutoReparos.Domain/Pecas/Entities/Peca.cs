using AutoReparos.Domain.Pecas.Exceptions;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Veiculos.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Domain.Pecas.Entities
{
    public class Peca : Entity
    {
        public string Nome { get; private set; }
        public string? Descricao { get; private set; }
        public decimal Valor { get; private set; }
        public int QuantidadeEstoque { get; private set; }
        public DateTime CriadoEm { get; }
        public DateTime? AtualizadoEm { get; private set; }

        protected Peca() { }

        public Peca(string nome, string? descricao, decimal valor, int quantidadeEstoque) : base()
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
                throw new InvalidPecaException("Quantidade deve ser maior que zero.");

            QuantidadeEstoque += quantidade;
            AtualizadoEm = DateTime.UtcNow;
        }

        public void RemoverEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new InvalidPecaException("Quantidade deve ser maior que zero.");

            if (quantidade > QuantidadeEstoque)
                throw new InvalidPecaException("Quantidade insuficiente em estoque.");

            QuantidadeEstoque -= quantidade;
            AtualizadoEm = DateTime.UtcNow;
        }

        private static void Validar(string nome, decimal valor, int quantidadeEstoque)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new InvalidPecaException("Nome é obrigatório.");

            if (nome.Length > 100)
                throw new InvalidPecaException("Nome deve ter no máximo 100 caracteres.");

            if (valor <= 0)
                throw new InvalidPecaException("Valor deve ser maior que zero.");

            if (quantidadeEstoque < 0)
                throw new InvalidPecaException("Quantidade em estoque não pode ser negativa.");
        }
    }
}
