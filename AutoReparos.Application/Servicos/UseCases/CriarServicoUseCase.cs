using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Servicos.Mappers;
using AutoReparos.Application.Servicos.UseCases.Interfaces;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;

namespace AutoReparos.Application.Servicos.UseCases
{
    public class CriarServicoUseCase(IServicoRepository repository) : ICriarServicoUseCase
    {
        public async Task<ServicoDto> ExecuteAsync(CriarServicoDto dto)
        {
            var servico = new Servico(dto.Nome, dto.Descricao, dto.ValorTabelado);
            await repository.Create(servico);
            return ServicoMapper.ToDto(servico);
        }
    }
}
