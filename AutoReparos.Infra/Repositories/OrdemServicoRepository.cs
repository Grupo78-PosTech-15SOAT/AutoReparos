using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;
using AutoReparos.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoReparos.Infra.Repositories
{
    public class OrdemServicoRepository : IOrdemServicoRepository
    {
        private readonly AppDbContext _context;

        public OrdemServicoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task Create(OrdemServico ordemServico)
        {
            await _context.OrdensServico.AddAsync(ordemServico);
            await _context.SaveChangesAsync();
        }

        public async Task<OrdemServico?> GetById(Guid id)
            => await _context.OrdensServico
                .Include(os => os.Servicos)
                .Include(os => os.Insumos)
                .FirstOrDefaultAsync(os => os.Id == id);

        public async Task<(IEnumerable<OrdemServico> Items, int Total)> GetAll(
            Guid? clienteId,
            Guid? veiculoId,
            EStatusOrdemServico? status,
            int skip,
            int take)
        {
            var query = _context.OrdensServico
                .Include(os => os.Servicos)
                .Include(os => os.Insumos)
                .AsQueryable();

            if (clienteId.HasValue)
                query = query.Where(os => os.ClienteId == clienteId);

            if (veiculoId.HasValue)
                query = query.Where(os => os.VeiculoId == veiculoId);

            if (status.HasValue)
                query = query.Where(os => os.Status == status);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(os => os.CriadoEm)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, total);
        }

        public async Task<(IEnumerable<OrdemServico> Items, int Total)> GetByDocumentoOuPlaca(string? documento, string? placa, int skip, int take)
        {
            var query = _context.OrdensServico
                .AsNoTracking()
                .Include(os => os.Servicos)
                .Include(os => os.Insumos)
                .AsQueryable();

            if (!string.IsNullOrEmpty(documento))
            {
                var doc = Documento.Normalizar(documento);
                query = query.Where(os => _context.Clientes.Any(c => c.Id == os.ClienteId && c.Documento.Valor == doc));
            }

            if (!string.IsNullOrEmpty(placa))
            {
                var placaNormalizada = Placa.Normalizar(placa);
                query = query.Where(os => _context.Veiculos.Any(v => v.Id == os.VeiculoId && v.Placa.Valor == placaNormalizada));
            }

            if (string.IsNullOrEmpty(documento) && string.IsNullOrEmpty(placa))
                return (Enumerable.Empty<OrdemServico>(), 0);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(os => os.CriadoEm)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, total);
        }

        public async Task Update(OrdemServico ordemServico)
        {
            _context.OrdensServico.Update(ordemServico);
            await _context.SaveChangesAsync();
        }
    }
}
