using System.Collections.Generic;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Strategies.Interfaces;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;

namespace AutoReparos.Application.OrdensServicos.Strategies
{
    public class ReceivedKanbanCardStrategy : IKanbanCardStrategy
    {
        public bool CanHandle(EStatusOrdemServico status) => status == EStatusOrdemServico.Recebida;
        public string ColumnKey => "Recebida";

        public KanbanCardDto CreateCard(OrdemServico os, IDictionary<string, string> usuarioDict)
        {
            var clienteNome = os.Cliente?.Nome ?? "Cliente não encontrado";
            var placaVeiculo = os.Veiculo?.Placa?.Valor ?? "Placa não encontrada";
            var modeloVeiculo = os.Veiculo?.Modelo ?? "Modelo não encontrado";

            return new ReceivedKanbanCardDto(
                os.Id, clienteNome, placaVeiculo, modeloVeiculo,
                os.Status.ToString(), os.CriadoEm);
        }
    }
}
