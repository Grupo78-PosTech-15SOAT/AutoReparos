using AutoReparos.Domain.Veiculos.Entities;

namespace AutoReparos.Domain.Veiculos.Repositories
{
    public interface IVeiculoRepository
    {
        Task Create(Veiculo veiculo);

        Task<(IEnumerable<Veiculo> Items, int Total)> GetAll(Guid? clienteId, int skip, int take);
        Task<Veiculo?> GetById(Guid id);
        Task<Veiculo?> GetByPlaca(string placa);

        Task Update(Veiculo veiculo);
        Task Delete(Veiculo veiculo);
        Task<IEnumerable<Veiculo>> GetByIds(IEnumerable<Guid> ids);
    }
}
