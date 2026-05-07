using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.DTOs.Response;
using AutoReparos.Application.Insumos.Services.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Insumos.Services
{
    public class InsumoService : IInsumoService
    {
        private readonly IInsumoRepository _repository;

        private const string mensagemException = "Insumo não encontrado.";

        public InsumoService(IInsumoRepository repository)
        {
            _repository = repository;
        }

        public async Task<InsumoDto> Create(CriarInsumoDto dto)
        {
            var insumo = new Insumo(dto.Nome, dto.Descricao, dto.Valor, dto.QuantidadeEstoque);
            await _repository.Create(insumo);
            return ToDTO(insumo);
        }

        public async Task<InsumoDto?> GetById(Guid id)
        {
            var insumo = await _repository.GetById(id);
            return insumo is null ? null : ToDTO(insumo);
        }

        public async Task<PagedResult<InsumoDto>> GetAll(InsumoPagedRequest request)
        {
            var (items, total) = await _repository.GetAll(request.Nome, request.Skip, request.PageSize);
            return new PagedResult<InsumoDto>(items.Select(ToDTO), total, request.PageNumber, request.PageSize);
        }

        public async Task Update(Guid id, AtualizarInsumoDto dto)
        {
            var insumo = await _repository.GetById(id)
                ?? throw new NotFoundException(mensagemException);

            insumo.Atualizar(dto.Nome, dto.Descricao, dto.Valor);
            await _repository.Update(insumo);
        }

        public async Task AdicionarEstoque(Guid id, AtualizarEstoqueDto dto)
        {
            var insumo = await _repository.GetById(id)
                ?? throw new NotFoundException(mensagemException);

            insumo.AdicionarEstoque(dto.Quantidade);
            await _repository.Update(insumo);
        }

        public async Task RemoverEstoque(Guid id, AtualizarEstoqueDto dto)
        {
            var insumo = await _repository.GetById(id)
                ?? throw new NotFoundException(mensagemException);

            insumo.RemoverEstoque(dto.Quantidade);
            await _repository.Update(insumo);
        }

        public async Task Delete(Guid id)
        {
            var insumo = await _repository.GetById(id)
                ?? throw new NotFoundException(mensagemException);

            await _repository.Delete(insumo);
        }

        private static InsumoDto ToDTO(Insumo p) => new(
            p.Id, p.Nome, p.Descricao, p.Valor, p.QuantidadeEstoque, p.CriadoEm, p.AtualizadoEm
        );
    }
}
