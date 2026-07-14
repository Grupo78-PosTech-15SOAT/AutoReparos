using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.UseCases;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Insumos
{
    public class AdicionarEstoqueUseCaseTests
    {
        private readonly IInsumoRepository _repository;
        private readonly AdicionarEstoqueUseCase _useCase;

        public AdicionarEstoqueUseCaseTests()
        {
            _repository = Substitute.For<IInsumoRepository>();
            _useCase = new AdicionarEstoqueUseCase(_repository);
        }

        [Fact(DisplayName = "AdicionarEstoque When Insumo Exists Should Increase Estoque")]
        public async Task AdicionarEstoque_WhenInsumoExists_ShouldIncreaseEstoque()
        {
            var insumo = new Insumo("Óleo 5W30", "Óleo sintético", 45m, 10);
            var dto = new AtualizarEstoqueDto(5);

            _repository.GetById(insumo.Id).Returns(insumo);

            await _useCase.ExecuteAsync(insumo.Id, dto);

            insumo.QuantidadeEstoque.Should().Be(15);
            await _repository.Received(1).Update(insumo);
        }

        [Fact(DisplayName = "AdicionarEstoque When Insumo Does Not Exist Should Throw NotFoundException")]
        public async Task AdicionarEstoque_WhenInsumoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            var dto = new AtualizarEstoqueDto(5);
            _repository.GetById(id).Returns((Insumo?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id, dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
