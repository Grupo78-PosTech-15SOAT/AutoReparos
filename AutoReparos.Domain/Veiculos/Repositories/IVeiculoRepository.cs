using AutoReparos.Domain.Veiculos.Entities;

namespace AutoReparos.Domain.Veiculos.Repositories
{
    public interface IVeiculoRepository
    {
        Task Create(Veiculo veiculo);
        Task Update(Veiculo veiculo);
        Task Delete(Veiculo veiculo);

        Task<Veiculo?> GetById(Guid id);
        Task<IEnumerable<Veiculo>> GetByClienteId(Guid clienteId);

        Task<Veiculo?> GetByPlaca(string placa);
    }
}
