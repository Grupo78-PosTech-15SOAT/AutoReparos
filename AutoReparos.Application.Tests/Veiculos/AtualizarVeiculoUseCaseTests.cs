using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.UseCases;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Veiculos
{
    public class AtualizarVeiculoUseCaseTests
    {
        private readonly IVeiculoRepository _repository;
        private readonly AtualizarVeiculoUseCase _useCase;

        public AtualizarVeiculoUseCaseTests()
        {
            _repository = Substitute.For<IVeiculoRepository>();
            _useCase = new AtualizarVeiculoUseCase(_repository);
        }

        [Fact(DisplayName = "Update When Veiculo Exists Should Update And Call Repository")]
        public async Task Update_WhenVeiculoExists_ShouldUpdate()
        {
            var veiculo = new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("ABC1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            var dto = new VeiculoUpdateDto("Chevrolet", "Onix LT", 2020, 2022);

            _repository.GetById(veiculo.Id).Returns(veiculo);

            await _useCase.ExecuteAsync(veiculo.Id, dto);

            veiculo.Modelo.Should().Be(dto.Modelo);
            veiculo.AnoModelo.Should().Be(dto.AnoModelo);
            await _repository.Received(1).Update(veiculo);
        }

        [Fact(DisplayName = "Update When Veiculo Does Not Exist Should Throw NotFoundException")]
        public async Task Update_WhenVeiculoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            var dto = new VeiculoUpdateDto("Chevrolet", "Onix LT", 2020, 2022);
            _repository.GetById(id).Returns((Veiculo?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id, dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
