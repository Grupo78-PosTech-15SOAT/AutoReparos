using AutoReparos.Domain.Pecas.Entities;
using AutoReparos.Domain.Pecas.Repositories;
using AutoReparos.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoReparos.Infra.Repositories
{
    public class PecaRepository : IPecaRepository
    {
        private readonly AppDbContext _context;

        public PecaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task Create(Peca peca)
        {
            await _context.Pecas.AddAsync(peca);
            await _context.SaveChangesAsync();
        }

        public async Task<Peca?> GetById(Guid id)
            => await _context.Pecas.FindAsync(id);

        public async Task<(IEnumerable<Peca> Items, int Total)> GetAll(string? nome, int skip, int take)
        {
            var query = _context.Pecas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(p => p.Nome.Contains(nome));

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Nome)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, total);
        }

        public async Task Update(Peca peca)
        {
            _context.Pecas.Update(peca);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Peca peca)
        {
            _context.Pecas.Remove(peca);
            await _context.SaveChangesAsync();
        }
    }
}
