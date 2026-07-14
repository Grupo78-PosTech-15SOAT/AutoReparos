using AutoReparos.Application.Veiculos.UseCases;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Veiculos
{
    public class ObterVeiculoPorPlacaUseCaseTests
    {
        private readonly IVeiculoRepository _repository;
        private readonly ObterVeiculoPorPlacaUseCase _useCase;

        public ObterVeiculoPorPlacaUseCaseTests()
        {
            _repository = Substitute.For<IVeiculoRepository>();
            _useCase = new ObterVeiculoPorPlacaUseCase(_repository);
        }

        [Fact(DisplayName = "GetByPlaca When Veiculo Exists Should Return Dto")]
        public async Task GetByPlaca_WhenVeiculoExists_ShouldReturnDto()
        {
            var veiculo = new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("ABC1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            _repository.GetByPlaca("ABC1D23").Returns(veiculo);

            var result = await _useCase.ExecuteAsync("ABC1D23");

            result.Should().NotBeNull();
            result!.Placa.Should().Be("ABC1D23");
        }

        [Fact(DisplayName = "GetByPlaca When Veiculo Does Not Exist Should Throw NotFoundException")]
        public async Task GetByPlaca_WhenVeiculoDoesNotExist_ShouldThrowNotFoundException()
        {
            _repository.GetByPlaca("XYZ9Z99").Returns((Veiculo?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync("XYZ9Z99");

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
