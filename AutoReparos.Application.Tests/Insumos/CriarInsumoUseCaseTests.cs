using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.UseCases;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Insumos
{
    public class CriarInsumoUseCaseTests
    {
        private readonly IInsumoRepository _repository;
        private readonly CriarInsumoUseCase _useCase;

        public CriarInsumoUseCaseTests()
        {
            _repository = Substitute.For<IInsumoRepository>();
            _useCase = new CriarInsumoUseCase(_repository);
        }

        [Fact(DisplayName = "Create Insumo With Valid Data Should Return Dto")]
        public async Task Create_WithValidData_ShouldReturnDto()
        {
            var dto = new CriarInsumoDto("Óleo 5W30", "Óleo sintético", 45m, 10);

            var result = await _useCase.ExecuteAsync(dto);

            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            result.QuantidadeEstoque.Should().Be(dto.QuantidadeEstoque);
            await _repository.Received(1).Create(Arg.Any<Insumo>());
        }
    }
}
