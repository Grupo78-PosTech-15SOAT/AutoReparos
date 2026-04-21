using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Infra.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
            return await _context.OrdensServicoServicos
                .Where(s => s.Status == EStatusServicoOS.Concluido
                    && s.IniciadoEm.HasValue
                    && s.ConcluidoEm.HasValue)
                .GroupBy(s => new { s.ServicoId })
                .Select(g => new
                {
                    ServicoId = g.Key.ServicoId,
                    NomeServico = _context.Servicos
                        .Where(s => s.Id == g.Key.ServicoId)
                        .Select(s => s.Nome)
                        .FirstOrDefault() ?? "",
                    TempoMedioTicks = g.Average(s =>
                        (s.ConcluidoEm!.Value - s.IniciadoEm!.Value).Ticks),
                    TotalExecucoes = g.Count()
                })
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(r => (
                    r.ServicoId,
                    r.NomeServico,
                    TimeSpan.FromTicks((long)r.TempoMedioTicks),
                    r.TotalExecucoes
                )));
        }

        public async Task<(Guid ServicoId, string NomeServico, TimeSpan TempoMedio, int TotalExecucoes)?> GetTempoMedioById(Guid id)
        {
            var result = await _context.OrdensServicoServicos
                .Where(s => s.ServicoId == id
                    && s.Status == EStatusServicoOS.Concluido
                    && s.IniciadoEm.HasValue
                    && s.ConcluidoEm.HasValue)
                .GroupBy(s => s.ServicoId)
                .Select(g => new
                {
                    ServicoId = g.Key,
                    TempoMedioTicks = g.Average(s =>
                        (s.ConcluidoEm!.Value - s.IniciadoEm!.Value).Ticks),
                    TotalExecucoes = g.Count()
                })
                .FirstOrDefaultAsync();

            if (result is null) return null;

            var servico = await _context.Servicos.FindAsync(id);

            return (
                result.ServicoId,
                servico?.Nome ?? "",
                TimeSpan.FromTicks((long)result.TempoMedioTicks),
                result.TotalExecucoes
            );
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
