using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.UseCases.Interfaces;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Servicos.UseCases
{
    public class AtualizarServicoUseCase(IServicoRepository repository) : IAtualizarServicoUseCase
    {
        public async Task ExecuteAsync(Guid id, AtualizarServicoDto dto)
        {
            var servico = await repository.GetById(id)
                ?? throw new NotFoundException("Serviço não encontrado.");

            servico.Atualizar(dto.Nome, dto.Descricao, dto.ValorTabelado);
            await repository.Update(servico);
        }
    }
}
