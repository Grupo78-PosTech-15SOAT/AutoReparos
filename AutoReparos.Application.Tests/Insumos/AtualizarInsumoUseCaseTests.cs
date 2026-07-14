using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.UseCases;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Insumos
{
    public class AtualizarInsumoUseCaseTests
    {
        private readonly IInsumoRepository _repository;
        private readonly AtualizarInsumoUseCase _useCase;

        public AtualizarInsumoUseCaseTests()
        {
            _repository = Substitute.For<IInsumoRepository>();
            _useCase = new AtualizarInsumoUseCase(_repository);
        }

        [Fact(DisplayName = "Update When Insumo Exists Should Update And Call Repository")]
        public async Task Update_WhenInsumoExists_ShouldUpdate()
        {
            var insumo = new Insumo("Óleo 5W30", "Óleo sintético", 45m, 10);
            var dto = new AtualizarInsumoDto("Óleo 5W40", "Nova descrição", 50m);

            _repository.GetById(insumo.Id).Returns(insumo);

            await _useCase.ExecuteAsync(insumo.Id, dto);

            insumo.Nome.Should().Be(dto.Nome);
            insumo.Valor.Should().Be(dto.Valor);
            await _repository.Received(1).Update(insumo);
        }

        [Fact(DisplayName = "Update When Insumo Does Not Exist Should Throw NotFoundException")]
        public async Task Update_WhenInsumoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            var dto = new AtualizarInsumoDto("Óleo 5W40", "Nova descrição", 50m);
            _repository.GetById(id).Returns((Insumo?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id, dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
