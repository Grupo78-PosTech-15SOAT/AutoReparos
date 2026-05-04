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

        public async Task<ServicoDTO> Create(CriarServicoDTO dto)
        {
            var servico = new Servico(dto.Nome, dto.Descricao, dto.ValorTabelado);
            await _repository.Create(servico);
            return ToDto(servico);
        }

        public async Task<ServicoDTO?> GetById(Guid id)
        {
            var servico = await _repository.GetById(id);
            return servico is null ? null : ToDto(servico);
        }

        public async Task<PagedResult<ServicoDTO>> GetAll(ServicoPagedRequest request)
        {
            var (servicos, total) = await _repository.GetAll(request.Nome, request.Skip, request.PageSize);
            return new PagedResult<ServicoDTO>(servicos.Select(ToDto), total, request.PageNumber, request.PageSize);
        }

        public async Task<IEnumerable<TempoMedioServicoDTO>> GetTempoMedio()
        {
            var result = await _repository.GetTempoMedio();
            return result.Select(r => new TempoMedioServicoDTO(r.ServicoId, r.NomeServico, r.TempoMedio, r.TotalExecucoes));
        }

        public async Task<TempoMedioServicoDTO?> GetTempoMedioById(Guid id)
        {
            var result = await _repository.GetTempoMedioById(id);
            return result is null ? null : new TempoMedioServicoDTO(result.Value.ServicoId, result.Value.NomeServico, result.Value.TempoMedio, result.Value.TotalExecucoes);
        }

        public async Task Update(Guid id, AtualizarServicoDTO dto)
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

        private static ServicoDTO ToDto(Servico s) => new(
            s.Id, s.Nome, s.Descricao, s.ValorTabelado, s.CriadoEm, s.AtualizadoEm
        );
    }
}
