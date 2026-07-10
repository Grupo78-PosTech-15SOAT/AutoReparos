using AutoReparos.Application.Insumos.UseCases;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Insumos
{
    public class ObterInsumoPorIdUseCaseTests
    {
        private readonly IInsumoRepository _repository;
        private readonly ObterInsumoPorIdUseCase _useCase;

        public ObterInsumoPorIdUseCaseTests()
        {
            _repository = Substitute.For<IInsumoRepository>();
            _useCase = new ObterInsumoPorIdUseCase(_repository);
        }

        [Fact(DisplayName = "GetById When Insumo Exists Should Return Dto")]
        public async Task GetById_WhenInsumoExists_ShouldReturnDto()
        {
            var insumo = new Insumo("Óleo 5W30", "Óleo sintético", 45m, 10);
            _repository.GetById(insumo.Id).Returns(insumo);

            var result = await _useCase.ExecuteAsync(insumo.Id);

            result.Should().NotBeNull();
            result!.Nome.Should().Be(insumo.Nome);
        }

        [Fact(DisplayName = "GetById When Insumo Does Not Exist Should Return Null")]
        public async Task GetById_WhenInsumoDoesNotExist_ShouldReturnNull()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((Insumo?)null);

            var result = await _useCase.ExecuteAsync(id);

            result.Should().BeNull();
        }
    }
}
