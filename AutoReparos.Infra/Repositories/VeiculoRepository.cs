using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Exceptions;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AutoReparos.Infra.Repositories
{
    public class VeiculoRepository : IVeiculoRepository
    {
        private readonly AppDbContext _context;

        public VeiculoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task Create(Veiculo veiculo)
        {
            try
            {
                _context.Veiculos.Add(veiculo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw HandleDuplicatedException(ex);
            }
        }

        static private Exception HandleDuplicatedException(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx &&
                pgEx.SqlState == "23505")
            {
                return pgEx.ConstraintName switch
                {
                    "IX_Veiculos_Placa" => new DuplicatedPlacaException(),
                    "IX_Veiculos_Chassi" => new DuplicatedChassiException(),
                    "IX_Veiculos_Renavam" => new DuplicatedRenavamException(),
                    _ => new NotImplementedException(pgEx.ConstraintName ?? "unknown")
                };
            }

            return ex;
        }

        public async Task<(IEnumerable<Veiculo> Items, int Total)> GetAll(Guid? clienteId, int skip, int take)
        {
            var query = _context.Veiculos.AsQueryable();

            if (clienteId.HasValue)
                query = query.Where(v => v.ClienteId == clienteId);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(v => v.Modelo)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Veiculo?> GetById(Guid id)
        {
            return await _context.Veiculos.FindAsync(id);
        }

        public async Task<Veiculo?> GetByPlaca(string placa)
        {
            return await _context.Veiculos.FirstOrDefaultAsync(v => v.Placa.Valor == placa);
        }

        public async Task Update(Veiculo veiculo)
        {
            _context.Veiculos.Update(veiculo);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Veiculo veiculo)
        {
            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();
        }
    }
}
