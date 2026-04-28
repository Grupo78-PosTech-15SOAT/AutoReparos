using AutoReparos.Domain.Pecas.Entities;

namespace AutoReparos.Domain.Pecas.Repositories
{
    public interface IPecaRepository
    {
        Task Create(Peca peca);
        Task<Peca?> GetById(Guid id);
        Task<(IEnumerable<Peca> Items, int Total)> GetAll(string? nome, int skip, int take);
        Task Update(Peca peca);
        Task Delete(Peca peca);
    }
}
