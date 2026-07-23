using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.UseCases.Core.Interfaces;
using AutoReparos.Application.Shared;
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
        public async Task<PagedResult<OrdemServicoKanbanDto>> ExecuteAsync(PagedRequest request)
        {
            var (items, total) = await ordemServicoRepository.GetKanban(request.Skip, request.PageSize);
            
            var dtos = new List<OrdemServicoKanbanDto>();
            foreach (var os in items)
            {
                var cliente = await clienteRepository.GetById(os.ClienteId);
                var veiculo = await veiculoRepository.GetById(os.VeiculoId);
                
                var clienteNome = cliente?.Nome ?? "Cliente não encontrado";
                var placaVeiculo = veiculo?.Placa?.Valor ?? "Placa não encontrada";
                var modeloVeiculo = veiculo?.Modelo ?? "Modelo não encontrado";
                var numeroOS = os.Id.ToString()[..8].ToUpper();
                
                var itensServico = os.Servicos.Select(s => new ItemServicoKanbanDto(s.Id, s.Status == AutoReparos.Domain.OrdensServicos.Enums.EStatusServicoOS.Concluido));
                
                dtos.Add(new OrdemServicoKanbanDto(
                    os.Id,
                    numeroOS,
                    clienteNome,
                    placaVeiculo,
                    modeloVeiculo,
                    os.Status.ToString(),
                    os.ValorTotal,
                    itensServico
                ));
            }
            
            return new PagedResult<OrdemServicoKanbanDto>(dtos, total, request.PageNumber, request.PageSize);
        }
    }
}
