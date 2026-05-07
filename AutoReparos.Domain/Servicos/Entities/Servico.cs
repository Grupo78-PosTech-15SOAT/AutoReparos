using AutoReparos.Domain.Servicos.exceptions;
using AutoReparos.Domain.Shared;

namespace AutoReparos.Domain.Servicos.Entities
{
    public class Servico : Entity
    {
        public string Nome { get; private set; } = null!;
        public string? Descricao { get; private set; }
        public decimal? ValorTabelado { get; private set; }
        public DateTime CriadoEm { get; }
        public DateTime? AtualizadoEm { get; private set; }

        protected Servico() { }

        public Servico(string nome, string? descricao, decimal? valorTabelado) : base()
        {
            Validar(nome, valorTabelado);

            Nome = nome;
            Descricao = descricao;
            ValorTabelado = valorTabelado;
            CriadoEm = DateTime.UtcNow;
        }

        public void Atualizar(string nome, string? descricao, decimal? valorTabelado)
        {
            Validar(nome, valorTabelado);

            Nome = nome;
            Descricao = descricao;
            ValorTabelado = valorTabelado;
            AtualizadoEm = DateTime.UtcNow;
        }

        private static void Validar(string nome, decimal? valorTabelado)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new InvalidServicoException("Nome é obrigatório.");

            if (nome.Length > 100)
                throw new InvalidServicoException("Nome deve ter no máximo 100 caracteres.");

            if (valorTabelado.HasValue && valorTabelado <= 0)
                throw new InvalidServicoException("Valor tabelado deve ser maior que zero.");
        }
    }
}
