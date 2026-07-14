using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.UseCases.Interfaces;

namespace AutoReparos.API.Controllers
{
    public class VeiculoController(
        ICriarVeiculoUseCase criarVeiculoUseCase,
        IListarVeiculosUseCase listarVeiculosUseCase,
        IObterVeiculoPorIdUseCase obterVeiculoPorIdUseCase,
        IObterVeiculoPorPlacaUseCase obterVeiculoPorPlacaUseCase,
        IAtualizarVeiculoUseCase atualizarVeiculoUseCase,
        IExcluirVeiculoUseCase excluirVeiculoUseCase)
    {
        public async Task<IResult> Create(VeiculoCreateDto dto)
        {
            var veiculo = await criarVeiculoUseCase.ExecuteAsync(dto);
            return Results.CreatedAtRoute("GetVeiculoById", new { id = veiculo.Id }, veiculo);
        }

        public async Task<IResult> GetAll(VeiculoPagedRequest request)
        {
            var result = await listarVeiculosUseCase.ExecuteAsync(request);
            return Results.Ok(result);
        }

        public async Task<IResult> GetById(Guid id)
        {
            var veiculo = await obterVeiculoPorIdUseCase.ExecuteAsync(id);
            return veiculo is null ? Results.NotFound() : Results.Ok(veiculo);
        }

        public async Task<IResult> GetByPlaca(string placa)
        {
            var veiculo = await obterVeiculoPorPlacaUseCase.ExecuteAsync(placa);
            return veiculo is null ? Results.NotFound() : Results.Ok(veiculo);
        }

        public async Task<IResult> Update(Guid id, VeiculoUpdateDto dto)
        {
            await atualizarVeiculoUseCase.ExecuteAsync(id, dto);
            return Results.NoContent();
        }

        public async Task<IResult> Delete(Guid id)
        {
            await excluirVeiculoUseCase.ExecuteAsync(id);
            return Results.NoContent();
        }
    }
}
