using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.UseCases.Interfaces;

namespace AutoReparos.API.Controllers
{
    public class ServicoController(
        ICriarServicoUseCase criarServicoUseCase,
        IObterServicoPorIdUseCase obterServicoPorIdUseCase,
        IListarServicosUseCase listarServicosUseCase,
        IObterTempoMedioServicosUseCase obterTempoMedioServicosUseCase,
        IObterTempoMedioServicoPorIdUseCase obterTempoMedioServicoPorIdUseCase,
        IAtualizarServicoUseCase atualizarServicoUseCase,
        IExcluirServicoUseCase excluirServicoUseCase)
    {
        public async Task<IResult> Create(CriarServicoDto dto)
        {
            var servico = await criarServicoUseCase.ExecuteAsync(dto);
            return Results.CreatedAtRoute("GetServicoById", new { id = servico.Id }, servico);
        }

        public async Task<IResult> GetById(Guid id)
        {
            var servico = await obterServicoPorIdUseCase.ExecuteAsync(id);
            return servico is null ? Results.NotFound() : Results.Ok(servico);
        }

        public async Task<IResult> GetAll(ServicoPagedRequest request)
        {
            var result = await listarServicosUseCase.ExecuteAsync(request);
            return Results.Ok(result);
        }

        public async Task<IResult> GetTempoMedio()
        {
            var result = await obterTempoMedioServicosUseCase.ExecuteAsync();
            return Results.Ok(result);
        }

        public async Task<IResult> GetTempoMedioById(Guid id)
        {
            var result = await obterTempoMedioServicoPorIdUseCase.ExecuteAsync(id);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }

        public async Task<IResult> Update(Guid id, AtualizarServicoDto dto)
        {
            await atualizarServicoUseCase.ExecuteAsync(id, dto);
            return Results.NoContent();
        }

        public async Task<IResult> Delete(Guid id)
        {
            await excluirServicoUseCase.ExecuteAsync(id);
            return Results.NoContent();
        }
    }
}
