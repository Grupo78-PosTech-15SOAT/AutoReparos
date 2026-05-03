using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Exceptions;
using AutoReparos.Domain.Shared;

namespace AutoReparos.Domain.OrdensServicos.Entities
{
    public class OrdemServicoInsumo : Entity
    {
        public Guid OrdemServicoId { get; private set; }
        public Guid? InsumoId { get; private set; }
        public string Descricao { get; private set; } = null!;
        public decimal ValorUnitario { get; private set; }
        public int Quantidade { get; private set; }
        public EOrigemInsumo Origem { get; private set; }

        public decimal ValorTotal => ValorUnitario * Quantidade;

        protected OrdemServicoInsumo() { }

        public OrdemServicoInsumo(
            Guid ordemServicoId,
            Guid? insumoId,
            string descricao,
            decimal valorUnitario,
            int quantidade,
            EOrigemInsumo origem) : base()
        {
            if (origem == EOrigemInsumo.Estoque && insumoId is null)
                throw new InvalidOrdemServicoException("Insumo do estoque deve ter referência ao cadastro.");

            if (string.IsNullOrWhiteSpace(descricao))
                throw new InvalidOrdemServicoException("Descrição é obrigatória.");

            if (valorUnitario <= 0)
                throw new InvalidOrdemServicoException("Valor unitário deve ser maior que zero.");

            if (quantidade <= 0)
                throw new InvalidOrdemServicoException("Quantidade deve ser maior que zero.");

            OrdemServicoId = ordemServicoId;
            InsumoId = insumoId;
            Descricao = descricao;
            ValorUnitario = valorUnitario;
            Quantidade = quantidade;
            Origem = origem;
        }

        public void AdicionarQuantidade(int quantidade)
        {
            if (quantidade <= 0)
                throw new InvalidOrdemServicoException("Quantidade deve ser maior que zero.");

            Quantidade += quantidade;
        }
    }
}
