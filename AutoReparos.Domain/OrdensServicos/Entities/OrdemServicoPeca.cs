using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Exceptions;
using AutoReparos.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Domain.OrdensServicos.Entities
{
    public class OrdemServicoPeca : Entity
    {
        public Guid OrdemServicoId { get; private set; }
        public Guid? PecaId { get; private set; }
        public string Descricao { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public int Quantidade { get; private set; }
        public EOrigemPeca Origem { get; private set; }

        public decimal ValorTotal => ValorUnitario * Quantidade;

        protected OrdemServicoPeca() { }

        public OrdemServicoPeca(
            Guid ordemServicoId,
            Guid? pecaId,
            string descricao,
            decimal valorUnitario,
            int quantidade,
            EOrigemPeca origem) : base()
        {
            if (origem == EOrigemPeca.Estoque && pecaId is null)
                throw new InvalidOrdemServicoException("Peça do estoque deve ter referência ao cadastro.");

            if (string.IsNullOrWhiteSpace(descricao))
                throw new InvalidOrdemServicoException("Descrição é obrigatória.");

            if (valorUnitario <= 0)
                throw new InvalidOrdemServicoException("Valor unitário deve ser maior que zero.");

            if (quantidade <= 0)
                throw new InvalidOrdemServicoException("Quantidade deve ser maior que zero.");

            OrdemServicoId = ordemServicoId;
            PecaId = pecaId;
            Descricao = descricao;
            ValorUnitario = valorUnitario;
            Quantidade = quantidade;
            Origem = origem;
        }
    }
}
