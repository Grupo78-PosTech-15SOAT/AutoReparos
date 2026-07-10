using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.UseCases;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Clientes
{
    public class AtualizarClienteUseCaseTests
    {
        private readonly IClienteRepository _repository;
        private readonly AtualizarClienteUseCase _useCase;

        public AtualizarClienteUseCaseTests()
        {
            _repository = Substitute.For<IClienteRepository>();
            _useCase = new AtualizarClienteUseCase(_repository);
        }

        [Fact(DisplayName = "Update When Cliente Exists Should Update And Call Repository")]
        public async Task Update_WhenClienteExists_ShouldUpdate()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var dto = new ClienteUpdateDto("João S. Silva", "11988888888", "joaosilva@teste.com");

            _repository.GetById(cliente.Id).Returns(cliente);

            await _useCase.ExecuteAsync(cliente.Id, dto);

            cliente.Nome.Should().Be(dto.Nome);
            cliente.Email.Endereco.Should().Be(dto.Email);
            await _repository.Received(1).Update(cliente);
        }

        [Fact(DisplayName = "Update When Cliente Does Not Exist Should Throw NotFoundException")]
        public async Task Update_WhenClienteDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            var dto = new ClienteUpdateDto("João S. Silva", "11988888888", "joaosilva@teste.com");
            _repository.GetById(id).Returns((Cliente?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id, dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
