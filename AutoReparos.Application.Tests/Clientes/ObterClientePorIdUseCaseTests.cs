using AutoReparos.Application.Clientes.UseCases;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Clientes
{
    public class ObterClientePorIdUseCaseTests
    {
        private readonly IClienteRepository _repository;
        private readonly ObterClientePorIdUseCase _useCase;

        public ObterClientePorIdUseCaseTests()
        {
            _repository = Substitute.For<IClienteRepository>();
            _useCase = new ObterClientePorIdUseCase(_repository);
        }

        [Fact(DisplayName = "GetById When Cliente Exists Should Return Dto")]
        public async Task GetById_WhenClienteExists_ShouldReturnDto()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            _repository.GetById(cliente.Id).Returns(cliente);

            var result = await _useCase.ExecuteAsync(cliente.Id);

            result.Should().NotBeNull();
            result!.Nome.Should().Be(cliente.Nome);
        }

        [Fact(DisplayName = "GetById When Cliente Does Not Exist Should Throw NotFoundException")]
        public async Task GetById_WhenClienteDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((Cliente?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
