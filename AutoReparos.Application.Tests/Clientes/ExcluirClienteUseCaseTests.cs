using AutoReparos.Application.Clientes.UseCases;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Clientes
{
    public class ExcluirClienteUseCaseTests
    {
        private readonly IClienteRepository _repository;
        private readonly ExcluirClienteUseCase _useCase;

        public ExcluirClienteUseCaseTests()
        {
            _repository = Substitute.For<IClienteRepository>();
            _useCase = new ExcluirClienteUseCase(_repository);
        }

        [Fact(DisplayName = "Delete When Cliente Exists Should Call Repository")]
        public async Task Delete_WhenClienteExists_ShouldCallDelete()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            _repository.GetById(cliente.Id).Returns(cliente);

            await _useCase.ExecuteAsync(cliente.Id);

            await _repository.Received(1).Delete(cliente);
        }

        [Fact(DisplayName = "Delete When Cliente Does Not Exist Should Throw NotFoundException")]
        public async Task Delete_WhenClienteDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((Cliente?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
