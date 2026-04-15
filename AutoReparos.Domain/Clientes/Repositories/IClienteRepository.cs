using AutoReparos.Domain.Clientes.Entities;

namespace AutoReparos.Domain.Clientes.Repositories
{
    public interface IClienteRepository
    {
        Task Create(Cliente cliente);
        Task<(IEnumerable<Cliente>, int Total)> GetAll(string? nome, int skip, int take);
        Task<Cliente?> GetById(Guid id);
        Task<Cliente?> GetByDocumentoOrEmail(string documento, string email);
        Task Update(Cliente cliente);
        Task Delete(Cliente cliente);
    }
}
