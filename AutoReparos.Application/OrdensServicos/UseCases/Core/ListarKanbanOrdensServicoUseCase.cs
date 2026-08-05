using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Strategies.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Usuarios.Repositories;

namespace AutoReparos.Application.OrdensServicos.UseCases.Core
{
    public class ListarKanbanOrdensServicoUseCase(
        IOrdemServicoRepository ordemServicoRepository,
        IUsuarioRepository usuarioRepository,
        IEnumerable<IKanbanCardStrategy> strategies) : IListarKanbanOrdensServicoUseCase
    {
        private const int MaxRegistrosKanban = 10000;

        public async Task<IEnumerable<KanbanColumnDto>> ExecuteAsync()
        {
            var (items, _) = await ordemServicoRepository.GetKanban(0, MaxRegistrosKanban);
            var (usuarios, _) = await usuarioRepository.GetAllAsync(null, 0, MaxRegistrosKanban);
            var usuarioDict = usuarios.ToDictionary(u => u.Id.ToString(), u => u.NomeCompleto);

            var columns = new Dictionary<string, List<KanbanCardDto>>
            {
                { "Recebida", new List<KanbanCardDto>() },
                { "Diagnostico", new List<KanbanCardDto>() },
                { "Aprovacao", new List<KanbanCardDto>() },
                { "Execucao", new List<KanbanCardDto>() },
                { "Finalizada", new List<KanbanCardDto>() }
            };

            foreach (var os in items)
            {
                if ((os.Status == EStatusOrdemServico.Finalizada || os.Status == EStatusOrdemServico.Entregue) &&
                    (os.FinalizadoEm == null || os.FinalizadoEm.Value.ToLocalTime().Date != DateTime.Today))
                {
                    continue;
                }

                var strategy = strategies.FirstOrDefault(s => s.CanHandle(os.Status));
                if (strategy != null)
                {
                    var card = strategy.CreateCard(os, usuarioDict);
                    columns[strategy.ColumnKey].Add(card);
                }
            }

            return columns.Select(kvp =>
            {
                if (kvp.Key == "Finalizada")
                {
                    var sortedCards = kvp.Value
                        .OfType<FinishedKanbanCardDto>()
                        .OrderBy(c => c.DataConclusao)
                        .Cast<KanbanCardDto>()
                        .ToList();
                    return new KanbanColumnDto(kvp.Key, sortedCards);
                }
                return new KanbanColumnDto(kvp.Key, kvp.Value);
            });
        }
    }
}
