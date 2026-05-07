using AutoReparos.Domain.Insumos.Entities;

namespace AutoReparos.Domain.Insumos.Repositories
{
    public interface IInsumoRepository
    {
        Task Create(Insumo insumo);
        Task<Insumo?> GetById(Guid id);
        Task<(IEnumerable<Insumo> Items, int Total)> GetAll(string? nome, int skip, int take);
        Task Update(Insumo insumo);
        Task Delete(Insumo insumo);
    }
}
