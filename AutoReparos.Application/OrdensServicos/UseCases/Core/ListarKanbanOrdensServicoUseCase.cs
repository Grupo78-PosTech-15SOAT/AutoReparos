using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Veiculos.Repositories;

namespace AutoReparos.Application.OrdensServicos.UseCases.Core
{
    public class ListarKanbanOrdensServicoUseCase(
        IOrdemServicoRepository ordemServicoRepository,
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository) : IListarKanbanOrdensServicoUseCase
    {
        public async Task<IEnumerable<KanbanColumnDto>> ExecuteAsync()
        {
            var (items, _) = await ordemServicoRepository.GetKanban(0, 1000);
            
            var clienteIds = items.Select(x => x.ClienteId).Distinct();
            var veiculoIds = items.Select(x => x.VeiculoId).Distinct();

            var clientes = (await clienteRepository.GetByIds(clienteIds)).ToDictionary(x => x.Id);
            var veiculos = (await veiculoRepository.GetByIds(veiculoIds)).ToDictionary(x => x.Id);

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
                if ((os.Status == AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.Finalizada || 
                     os.Status == AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.Entregue) && 
                    (os.FinalizadoEm == null || os.FinalizadoEm.Value.ToLocalTime().Date != DateTime.Today))
                {
                    continue;
                }

                var cliente = clientes.GetValueOrDefault(os.ClienteId);
                var veiculo = veiculos.GetValueOrDefault(os.VeiculoId);
                
                var clienteNome = cliente?.Nome ?? "Cliente não encontrado";
                var placaVeiculo = veiculo?.Placa?.Valor ?? "Placa não encontrada";
                var modeloVeiculo = veiculo?.Modelo ?? "Modelo não encontrado";
                var numeroOS = os.Id.ToString()[..8].ToUpper();
                
                KanbanCardDto card = os.Status switch
                {
                    AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.Recebida =>
                        new ReceivedKanbanCardDto(
                            os.Id, numeroOS, clienteNome, placaVeiculo, modeloVeiculo, 
                            os.Status.ToString(), os.CriadoEm),
                            
                    AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.EmDiagnostico =>
                        new DiagnosisKanbanCardDto(
                            os.Id, numeroOS, clienteNome, placaVeiculo, modeloVeiculo, 
                            os.Status.ToString(), os.ResponsavelId),
                            
                    AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.AguardandoAprovacao =>
                        new ApprovalKanbanCardDto(
                            os.Id, numeroOS, clienteNome, placaVeiculo, modeloVeiculo, 
                            os.Status.ToString(), os.ValorTotal, os.Servicos.Count, os.EnvioAprovacaoEm),
                            
                    AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.EmExecucao =>
                        new ExecutionKanbanCardDto(
                            os.Id, numeroOS, clienteNome, placaVeiculo, modeloVeiculo, 
                            os.Status.ToString(), os.ValorTotal, 
                            os.Servicos.Count > 0 
                                ? (double)os.Servicos.Count(s => s.Status == AutoReparos.Domain.OrdensServicos.Enums.EStatusServicoOS.Concluido) / os.Servicos.Count * 100 
                                : 0, 
                            "Mecânico Chefe",
                            os.Servicos.Count(s => s.Status == AutoReparos.Domain.OrdensServicos.Enums.EStatusServicoOS.Concluido),
                            os.Servicos.Count),
                            
                    AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.Finalizada or 
                    AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.Entregue =>
                        new FinishedKanbanCardDto(
                            os.Id, numeroOS, clienteNome, placaVeiculo, modeloVeiculo, 
                            os.Status.ToString(), os.ValorTotal, os.FinalizadoEm ?? DateTime.UtcNow, 
                            os.Status == AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.Entregue ? "Entregue" : "Aguardando Entrega"),
                            
                    _ => null!
                };

                if (card != null)
                {
                    string? column = os.Status switch
                    {
                        AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.Recebida => "Recebida",
                        AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.EmDiagnostico => "Diagnostico",
                        AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.AguardandoAprovacao => "Aprovacao",
                        AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.EmExecucao => "Execucao",
                        AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.Finalizada or 
                        AutoReparos.Domain.OrdensServicos.Enums.EStatusOrdemServico.Entregue => "Finalizada",
                        _ => null
                    };

                    if (column != null)
                    {
                        columns[column].Add(card);
                    }
                }
            }
            
            return columns.Select(kvp => new KanbanColumnDto(kvp.Key, kvp.Value));
        }
    }
}
