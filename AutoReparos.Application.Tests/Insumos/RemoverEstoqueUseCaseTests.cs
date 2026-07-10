using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.UseCases;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Exceptions;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Insumos
{
    public class RemoverEstoqueUseCaseTests
    {
        private readonly IInsumoRepository _repository;
        private readonly RemoverEstoqueUseCase _useCase;

        public RemoverEstoqueUseCaseTests()
        {
            _repository = Substitute.For<IInsumoRepository>();
            _useCase = new RemoverEstoqueUseCase(_repository);
        }

        [Fact(DisplayName = "RemoverEstoque When Insumo Exists Should Decrease Estoque")]
        public async Task RemoverEstoque_WhenInsumoExists_ShouldDecreaseEstoque()
        {
            var insumo = new Insumo("Óleo 5W30", "Óleo sintético", 45m, 10);
            var dto = new AtualizarEstoqueDto(5);

            _repository.GetById(insumo.Id).Returns(insumo);

            await _useCase.ExecuteAsync(insumo.Id, dto);

            insumo.QuantidadeEstoque.Should().Be(5);
            await _repository.Received(1).Update(insumo);
        }

        [Fact(DisplayName = "RemoverEstoque When Quantidade Insuficiente Should Throw InvalidInsumoException")]
        public async Task RemoverEstoque_WhenQuantidadeInsuficiente_ShouldThrowInvalidInsumoException()
        {
            var insumo = new Insumo("Óleo 5W30", "Óleo sintético", 45m, 2);
            var dto = new AtualizarEstoqueDto(5);

            _repository.GetById(insumo.Id).Returns(insumo);

            Func<Task> action = async () => await _useCase.ExecuteAsync(insumo.Id, dto);

            await action.Should().ThrowAsync<InvalidInsumoException>();
        }

        [Fact(DisplayName = "RemoverEstoque When Insumo Does Not Exist Should Throw NotFoundException")]
        public async Task RemoverEstoque_WhenInsumoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            var dto = new AtualizarEstoqueDto(5);
            _repository.GetById(id).Returns((Insumo?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id, dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
