using AutoReparos.Application.Veiculos.UseCases;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Veiculos
{
    public class ObterVeiculoPorIdUseCaseTests
    {
        private readonly IVeiculoRepository _repository;
        private readonly ObterVeiculoPorIdUseCase _useCase;

        public ObterVeiculoPorIdUseCaseTests()
        {
            _repository = Substitute.For<IVeiculoRepository>();
            _useCase = new ObterVeiculoPorIdUseCase(_repository);
        }

        [Fact(DisplayName = "GetById When Veiculo Exists Should Return Dto")]
        public async Task GetById_WhenVeiculoExists_ShouldReturnDto()
        {
            var veiculo = new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("ABC1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            _repository.GetById(veiculo.Id).Returns(veiculo);

            var result = await _useCase.ExecuteAsync(veiculo.Id);

            result.Should().NotBeNull();
            result!.Modelo.Should().Be(veiculo.Modelo);
        }

        [Fact(DisplayName = "GetById When Veiculo Does Not Exist Should Throw NotFoundException")]
        public async Task GetById_WhenVeiculoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((Veiculo?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
