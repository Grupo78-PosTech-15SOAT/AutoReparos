using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoReparos.Infra.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _context;

        public ClienteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task Create(Cliente cliente)
        {
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Cliente>, int Total)> GetAll(string? nome, int skip, int take)
        {
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrEmpty(nome))
            {
                query = query.Where(c => c.Nome.Contains(nome));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.Nome)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Cliente?> GetByDocumentoOrEmail(string documento, string email)
        {
            return await _context.Clientes.FirstOrDefaultAsync(c => c.Documento.Valor == documento || c.Email.Endereco == email);
        }

        public async Task<Cliente?> GetById(Guid id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task<IEnumerable<Cliente>> GetByIds(IEnumerable<Guid> ids)
        {
            return await _context.Clientes.Where(c => ids.Contains(c.Id)).ToListAsync();
        }

        public async Task Update(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Cliente cliente)
        {
            try
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new RelatedEntityException("clientes", "veículos ou ordens de serviços");
            }
        }
    }
}
