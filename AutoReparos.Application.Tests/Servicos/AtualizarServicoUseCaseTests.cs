using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.UseCases;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Servicos
{
    public class AtualizarServicoUseCaseTests
    {
        private readonly IServicoRepository _repository;
        private readonly AtualizarServicoUseCase _useCase;

        public AtualizarServicoUseCaseTests()
        {
            _repository = Substitute.For<IServicoRepository>();
            _useCase = new AtualizarServicoUseCase(_repository);
        }

        [Fact(DisplayName = "Update When Servico Exists Should Update And Call Repository")]
        public async Task Update_WhenServicoExists_ShouldUpdate()
        {
            var servico = new Servico("Troca de óleo", "Descrição", 150m);
            var dto = new AtualizarServicoDto("Troca de óleo sintético", "Nova descrição", 200m);

            _repository.GetById(servico.Id).Returns(servico);

            await _useCase.ExecuteAsync(servico.Id, dto);

            servico.Nome.Should().Be(dto.Nome);
            servico.ValorTabelado.Should().Be(dto.ValorTabelado);
            await _repository.Received(1).Update(servico);
        }

        [Fact(DisplayName = "Update When Servico Does Not Exist Should Throw NotFoundException")]
        public async Task Update_WhenServicoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            var dto = new AtualizarServicoDto("Troca de óleo sintético", "Nova descrição", 200m);
            _repository.GetById(id).Returns((Servico?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id, dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
