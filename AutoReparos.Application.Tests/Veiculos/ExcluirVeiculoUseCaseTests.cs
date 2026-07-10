using AutoReparos.Application.Veiculos.UseCases;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Veiculos
{
    public class ExcluirVeiculoUseCaseTests
    {
        private readonly IVeiculoRepository _repository;
        private readonly ExcluirVeiculoUseCase _useCase;

        public ExcluirVeiculoUseCaseTests()
        {
            _repository = Substitute.For<IVeiculoRepository>();
            _useCase = new ExcluirVeiculoUseCase(_repository);
        }

        [Fact(DisplayName = "Delete When Veiculo Exists Should Call Repository")]
        public async Task Delete_WhenVeiculoExists_ShouldCallDelete()
        {
            var veiculo = new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("ABC1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            _repository.GetById(veiculo.Id).Returns(veiculo);

            await _useCase.ExecuteAsync(veiculo.Id);

            await _repository.Received(1).Delete(veiculo);
        }

        [Fact(DisplayName = "Delete When Veiculo Does Not Exist Should Throw NotFoundException")]
        public async Task Delete_WhenVeiculoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((Veiculo?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
