using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Infra.Data;
using Microsoft.EntityFrameworkCore;

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
            await _context.Veiculos.AddAsync(veiculo);
            await _context.SaveChangesAsync();
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

        public async Task<Veiculo?> GetByPlaca(string placa)
        {
            return await _context.Veiculos
                .FirstOrDefaultAsync(v => v.Placa.Valor == placa);
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
