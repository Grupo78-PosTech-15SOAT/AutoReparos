using AutoReparos.Application.Servicos.UseCases;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Servicos
{
    public class ExcluirServicoUseCaseTests
    {
        private readonly IServicoRepository _repository;
        private readonly ExcluirServicoUseCase _useCase;

        public ExcluirServicoUseCaseTests()
        {
            _repository = Substitute.For<IServicoRepository>();
            _useCase = new ExcluirServicoUseCase(_repository);
        }

        [Fact(DisplayName = "Delete When Servico Exists Should Call Repository")]
        public async Task Delete_WhenServicoExists_ShouldCallDelete()
        {
            var servico = new Servico("Troca de óleo", "Descrição", 150m);
            _repository.GetById(servico.Id).Returns(servico);

            await _useCase.ExecuteAsync(servico.Id);

            await _repository.Received(1).Delete(servico);
        }

        [Fact(DisplayName = "Delete When Servico Does Not Exist Should Throw NotFoundException")]
        public async Task Delete_WhenServicoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((Servico?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
