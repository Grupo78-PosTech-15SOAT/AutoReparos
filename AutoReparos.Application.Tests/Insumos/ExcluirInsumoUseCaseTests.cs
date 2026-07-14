using AutoReparos.Application.Insumos.UseCases;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Insumos
{
    public class ExcluirInsumoUseCaseTests
    {
        private readonly IInsumoRepository _repository;
        private readonly ExcluirInsumoUseCase _useCase;

        public ExcluirInsumoUseCaseTests()
        {
            _repository = Substitute.For<IInsumoRepository>();
            _useCase = new ExcluirInsumoUseCase(_repository);
        }

        [Fact(DisplayName = "Delete When Insumo Exists Should Call Repository")]
        public async Task Delete_WhenInsumoExists_ShouldCallDelete()
        {
            var insumo = new Insumo("Óleo 5W30", "Óleo sintético", 45m, 10);
            _repository.GetById(insumo.Id).Returns(insumo);

            await _useCase.ExecuteAsync(insumo.Id);

            await _repository.Received(1).Delete(insumo);
        }

        [Fact(DisplayName = "Delete When Insumo Does Not Exist Should Throw NotFoundException")]
        public async Task Delete_WhenInsumoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((Insumo?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
