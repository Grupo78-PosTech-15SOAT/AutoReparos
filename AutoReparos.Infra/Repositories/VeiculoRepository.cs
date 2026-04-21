using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects.Exceptions;
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

        private Exception HandleDuplicatedException(DbUpdateException ex)
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

        public async Task<Veiculo?> GetById(Guid id)
        {
            return await _context.Veiculos.FindAsync(id);
        }

        public async Task<IEnumerable<Veiculo>> GetByClienteId(Guid clienteId)
        {
            return await _context.Veiculos
                .Where(v => v.ClienteId == clienteId)
                .ToListAsync();
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

        public async Task<Veiculo?> GetByPlaca(string placa)
        {
            return await _context.Veiculos.FirstOrDefaultAsync(v => v.Placa.Valor == placa);
        }
    }
}
