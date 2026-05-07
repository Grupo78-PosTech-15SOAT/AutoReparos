using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoReparos.Infra.Repositories
{
    public class ServicoRepository : IServicoRepository
    {
        private readonly AppDbContext _context;

        public ServicoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task Create(Servico servico)
        {
            await _context.Servicos.AddAsync(servico);
            await _context.SaveChangesAsync();
        }

        public async Task<Servico?> GetById(Guid id)
            => await _context.Servicos.FindAsync(id);

        public async Task<(IEnumerable<Servico> Items, int Total)> GetAll(
            string? nome, int skip, int take)
        {
            var query = _context.Servicos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(s => s.Nome.Contains(nome));

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.Nome)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IEnumerable<(Guid ServicoId, string NomeServico, TimeSpan TempoMedio, int TotalExecucoes)>> GetTempoMedio()
        {
            var execucoes = await _context.OrdensServicoServicos
                .Where(s => s.Status == EStatusServicoOS.Concluido
                    && s.IniciadoEm.HasValue
                    && s.ConcluidoEm.HasValue)
                .Select(s => new
                {
                    s.ServicoId,
                    s.IniciadoEm,
                    s.ConcluidoEm
                })
                .ToListAsync();

            var servicoIds = execucoes.Select(e => e.ServicoId).Distinct().ToList();
            var servicos = await _context.Servicos
                .Where(s => servicoIds.Contains(s.Id))
                .Select(s => new { s.Id, s.Nome })
                .ToListAsync();

            return execucoes
                .GroupBy(e => e.ServicoId)
                .Select(g => (
                    g.Key,
                    servicos.FirstOrDefault(s => s.Id == g.Key)?.Nome ?? "",
                    TimeSpan.FromTicks((long)g.Average(e =>
                        (e.ConcluidoEm!.Value - e.IniciadoEm!.Value).Ticks)),
                    g.Count()
                ));
        }

        public async Task<(Guid ServicoId, string NomeServico, TimeSpan TempoMedio, int TotalExecucoes)?> GetTempoMedioById(Guid id)
        {
            var execucoes = await _context.OrdensServicoServicos
                .Where(s => s.ServicoId == id
                    && s.Status == EStatusServicoOS.Concluido
                    && s.IniciadoEm.HasValue
                    && s.ConcluidoEm.HasValue)
                .Select(s => new
                {
                    s.ServicoId,
                    s.IniciadoEm,
                    s.ConcluidoEm
                })
                .ToListAsync();

            if (!execucoes.Any())
                return null;

            var servico = await _context.Servicos
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Nome
                })
                .FirstOrDefaultAsync();

            var resultado = execucoes
                .GroupBy(e => e.ServicoId)
                .Select(g => (
                    ServicoId: g.Key,
                    NomeServico: servico?.Nome ?? "",
                    TempoMedio: TimeSpan.FromTicks((long)g.Average(e =>
                        (e.ConcluidoEm!.Value - e.IniciadoEm!.Value).Ticks)),
                    TotalExecucoes: g.Count()
                ))
                .First();

            return resultado;
        }

        public async Task Update(Servico servico)
        {
            _context.Servicos.Update(servico);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Servico servico)
        {
            _context.Servicos.Remove(servico);
            await _context.SaveChangesAsync();
        }
    }
}
