using AutoReparos.Application.Pecas.DTOs.Request;
using AutoReparos.Application.Pecas.DTOs.Response;
using AutoReparos.Application.Pecas.Services.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.Pecas.Entities;
using AutoReparos.Domain.Pecas.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Application.Pecas.Services
{
    public class PecaService : IPecaService
    {
        private readonly IPecaRepository _repository;

        public PecaService(IPecaRepository repository)
        {
            _repository = repository;
        }

        public async Task<PecaDTO> Create(CriarPecaDTO dto)
        {
            var peca = new Peca(dto.Nome, dto.Descricao, dto.Valor, dto.QuantidadeEstoque);
            await _repository.Create(peca);
            return ToDTO(peca);
        }

        public async Task<PecaDTO?> GetById(Guid id)
        {
            var peca = await _repository.GetById(id);
            return peca is null ? null : ToDTO(peca);
        }

        public async Task<PagedResult<PecaDTO>> GetAll(string? nome, int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            var (items, total) = await _repository.GetAll(nome, skip, pageSize);
            return new PagedResult<PecaDTO>(items.Select(ToDTO), total, pageNumber, pageSize);
        }

        public async Task Update(Guid id, AtualizarPecaDTO dto)
        {
            var peca = await _repository.GetById(id)
                ?? throw new NotFoundException("Peça não encontrada.");

            peca.Atualizar(dto.Nome, dto.Descricao, dto.Valor);
            await _repository.Update(peca);
        }

        public async Task AdicionarEstoque(Guid id, AtualizarEstoqueDTO dto)
        {
            var peca = await _repository.GetById(id)
                ?? throw new NotFoundException("Peça não encontrada.");

            peca.AdicionarEstoque(dto.Quantidade);
            await _repository.Update(peca);
        }

        public async Task RemoverEstoque(Guid id, AtualizarEstoqueDTO dto)
        {
            var peca = await _repository.GetById(id)
                ?? throw new NotFoundException("Peça não encontrada.");

            peca.RemoverEstoque(dto.Quantidade);
            await _repository.Update(peca);
        }

        public async Task Delete(Guid id)
        {
            var peca = await _repository.GetById(id)
                ?? throw new NotFoundException("Peça não encontrada.");

            await _repository.Delete(peca);
        }

        private static PecaDTO ToDTO(Peca p) => new(
            p.Id, p.Nome, p.Descricao, p.Valor, p.QuantidadeEstoque, p.CriadoEm, p.AtualizadoEm
        );
    }
}
