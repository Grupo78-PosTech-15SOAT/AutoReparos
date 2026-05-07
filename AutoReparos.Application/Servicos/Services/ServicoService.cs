using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Servicos.Services.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Servicos.Services
{
    public class ServicoService : IServicoService
    {
        private readonly IServicoRepository _repository;

        public ServicoService(IServicoRepository repository)
        {
            _repository = repository;
        }

        public async Task<ServicoDto> Create(CriarServicoDto dto)
        {
            var servico = new Servico(dto.Nome, dto.Descricao, dto.ValorTabelado);
            await _repository.Create(servico);
            return ToDto(servico);
        }

        public async Task<ServicoDto?> GetById(Guid id)
        {
            var servico = await _repository.GetById(id);
            return servico is null ? null : ToDto(servico);
        }

        public async Task<PagedResult<ServicoDto>> GetAll(ServicoPagedRequest request)
        {
            var (servicos, total) = await _repository.GetAll(request.Nome, request.Skip, request.PageSize);
            return new PagedResult<ServicoDto>(servicos.Select(ToDto), total, request.PageNumber, request.PageSize);
        }

        public async Task<IEnumerable<TempoMedioServicoDto>> GetTempoMedio()
        {
            var result = await _repository.GetTempoMedio();
            return result.Select(r => new TempoMedioServicoDto(r.ServicoId, r.NomeServico, r.TempoMedio, r.TotalExecucoes));
        }

        public async Task<TempoMedioServicoDto?> GetTempoMedioById(Guid id)
        {
            var result = await _repository.GetTempoMedioById(id);
            return result is null ? null : new TempoMedioServicoDto(result.Value.ServicoId, result.Value.NomeServico, result.Value.TempoMedio, result.Value.TotalExecucoes);
        }

        public async Task Update(Guid id, AtualizarServicoDto dto)
        {
            var servico = await _repository.GetById(id)
                ?? throw new NotFoundException("Serviço não encontrado.");

            servico.Atualizar(dto.Nome, dto.Descricao, dto.ValorTabelado);
            await _repository.Update(servico);
        }

        public async Task Delete(Guid id)
        {
            var servico = await _repository.GetById(id)
                ?? throw new NotFoundException("Serviço não encontrado.");

            await _repository.Delete(servico);
        }

        private static ServicoDto ToDto(Servico s) => new(
            s.Id, s.Nome, s.Descricao, s.ValorTabelado, s.CriadoEm, s.AtualizadoEm
        );
    }
}
