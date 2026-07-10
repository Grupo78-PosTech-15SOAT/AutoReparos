using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.UseCases.Interfaces;

namespace AutoReparos.API.Controllers
{
    public class InsumoController(
        ICriarInsumoUseCase criarInsumoUseCase,
        IObterInsumoPorIdUseCase obterInsumoPorIdUseCase,
        IListarInsumosUseCase listarInsumosUseCase,
        IAtualizarInsumoUseCase atualizarInsumoUseCase,
        IAdicionarEstoqueUseCase adicionarEstoqueUseCase,
        IRemoverEstoqueUseCase removerEstoqueUseCase,
        IExcluirInsumoUseCase excluirInsumoUseCase)
    {
        public async Task<IResult> Create(CriarInsumoDto dto)
        {
            var insumo = await criarInsumoUseCase.ExecuteAsync(dto);
            return Results.CreatedAtRoute("GetInsumoById", new { id = insumo.Id }, insumo);
        }

        public async Task<IResult> GetById(Guid id)
        {
            var insumo = await obterInsumoPorIdUseCase.ExecuteAsync(id);
            return insumo is null ? Results.NotFound() : Results.Ok(insumo);
        }

        public async Task<IResult> GetAll(InsumoPagedRequest request)
        {
            var result = await listarInsumosUseCase.ExecuteAsync(request);
            return Results.Ok(result);
        }

        public async Task<IResult> Update(Guid id, AtualizarInsumoDto dto)
        {
            await atualizarInsumoUseCase.ExecuteAsync(id, dto);
            return Results.NoContent();
        }

        public async Task<IResult> AdicionarEstoque(Guid id, AtualizarEstoqueDto dto)
        {
            await adicionarEstoqueUseCase.ExecuteAsync(id, dto);
            return Results.NoContent();
        }

        public async Task<IResult> RemoverEstoque(Guid id, AtualizarEstoqueDto dto)
        {
            await removerEstoqueUseCase.ExecuteAsync(id, dto);
            return Results.NoContent();
        }

        public async Task<IResult> Delete(Guid id)
        {
            await excluirInsumoUseCase.ExecuteAsync(id);
            return Results.NoContent();
        }
    }
}
