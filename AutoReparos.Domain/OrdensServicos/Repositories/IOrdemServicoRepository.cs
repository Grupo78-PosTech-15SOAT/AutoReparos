using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;

namespace AutoReparos.Domain.OrdensServicos.Repositories
{
    public interface IOrdemServicoRepository
    {
        Task Create(OrdemServico ordemServico);
        Task<OrdemServico?> GetById(Guid id);
        Task<(IEnumerable<OrdemServico> Items, int Total)> GetAll(Guid? clienteId, Guid? veiculoId, EStatusOrdemServico? status, int skip, int take);
        Task Update(OrdemServico ordemServico);
    }
}
