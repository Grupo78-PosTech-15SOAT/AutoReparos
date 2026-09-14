using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Veiculos.Repositories;

namespace AutoReparos.Application.OrdensServicos.UseCases;

public class ObterMinhasOrdensServicoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IVeiculoRepository veiculoRepository) : IObterMinhasOrdensServicoUseCase
{
    public async Task<IEnumerable<MinhaOrdemServicoDto>> ExecuteAsync(
        Guid clienteId,
        string? placa = null,
        CancellationToken cancellationToken = default)
    {
        if (clienteId == Guid.Empty)
            return Enumerable.Empty<MinhaOrdemServicoDto>();

        Guid? veiculoFiltroId = null;

        // Se uma placa específica foi informada, valida propriedade do veículo
        if (!string.IsNullOrWhiteSpace(placa))
        {
            var veiculo = await veiculoRepository.GetByPlaca(placa.Trim());

            // Regra Zero-Trust: se o veículo não existe ou não pertence a este cliente, retorna lista vazia imediatamente
            if (veiculo == null || veiculo.ClienteId != clienteId)
            {
                return Enumerable.Empty<MinhaOrdemServicoDto>();
            }

            veiculoFiltroId = veiculo.Id;
        }

        // Busca as ordens de serviço do cliente (e opcionalmente filtradas pelo veículo do cliente)
        var (items, _) = await ordemServicoRepository.GetAll(
            clienteId: clienteId,
            veiculoId: veiculoFiltroId,
            status: null,
            skip: 0,
            take: 100
        );

        var osList = items.ToList();
        var veiculoIds = osList.Select(o => o.VeiculoId).Distinct();
        var veiculos = await veiculoRepository.GetByIds(veiculoIds);
        var veiculoDict = veiculos.ToDictionary(v => v.Id);

        return osList.Select(os =>
        {
            veiculoDict.TryGetValue(os.VeiculoId, out var veiculo);

            return new MinhaOrdemServicoDto(
                Id: os.Id,
                VeiculoId: os.VeiculoId,
                ModeloVeiculo: veiculo?.Modelo,
                PlacaVeiculo: veiculo?.Placa?.Valor,
                Status: os.Status.ToString(),
                Observacao: os.Observacao,
                ValorTotal: os.ValorTotal,
                CriadoEm: os.CriadoEm,
                IniciadoEm: os.IniciadoEm,
                FinalizadoEm: os.FinalizadoEm,
                EntregueEm: os.EntregueEm
            );
        });
    }
}
