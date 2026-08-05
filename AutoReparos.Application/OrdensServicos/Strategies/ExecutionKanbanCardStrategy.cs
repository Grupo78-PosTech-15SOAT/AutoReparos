using System.Collections.Generic;
using System.Linq;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Strategies.Interfaces;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;

namespace AutoReparos.Application.OrdensServicos.Strategies
{
    public class ExecutionKanbanCardStrategy : IKanbanCardStrategy
    {
        public bool CanHandle(EStatusOrdemServico status) => status == EStatusOrdemServico.EmExecucao;
        public string ColumnKey => "Execucao";

        public KanbanCardDto CreateCard(OrdemServico os, IDictionary<string, string> usuarioDict)
        {
            var clienteNome = os.Cliente?.Nome ?? "Cliente não encontrado";
            var placaVeiculo = os.Veiculo?.Placa?.Valor ?? "Placa não encontrada";
            var modeloVeiculo = os.Veiculo?.Modelo ?? "Modelo não encontrado";

            var mecanicoNome = !string.IsNullOrEmpty(os.ResponsavelId) && usuarioDict.TryGetValue(os.ResponsavelId, out var nome)
                ? nome
                : (os.ResponsavelId ?? "Mecânico Responsável");

            var servicosConcluidos = os.Servicos.Count(s => s.Status == EStatusServicoOS.Concluido);
            var servicosTotal = os.Servicos.Count;
            var progressoServicos = servicosTotal > 0 ? (double)servicosConcluidos / servicosTotal * 100 : 0;

            return new ExecutionKanbanCardDto(
                os.Id, clienteNome, placaVeiculo, modeloVeiculo,
                os.Status.ToString(), os.ValorTotal, progressoServicos,
                mecanicoNome, servicosConcluidos, servicosTotal);
        }
    }
}
