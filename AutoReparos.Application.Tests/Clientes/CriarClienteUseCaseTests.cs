using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.UseCases;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Exceptions;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Clientes
{
    public class CriarClienteUseCaseTests
    {
        private const string CpfValido = "52998224725";

        private readonly IClienteRepository _repository;
        private readonly CriarClienteUseCase _useCase;

        public CriarClienteUseCaseTests()
        {
            _repository = Substitute.For<IClienteRepository>();
            _useCase = new CriarClienteUseCase(_repository);
        }

        [Fact(DisplayName = "Create Cliente With Valid Data Should Return Dto")]
        public async Task Create_WithValidData_ShouldReturnDto()
        {
            var dto = new ClienteCreateDto("João Silva", CpfValido, "11999999999", "joao@teste.com");
            _repository.GetByDocumentoOrEmail(Arg.Any<string>(), Arg.Any<string>()).Returns((Cliente?)null);

            var result = await _useCase.ExecuteAsync(dto);

            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            result.Email.Should().Be(dto.Email);
            await _repository.Received(1).Create(Arg.Any<Cliente>());
        }

        [Fact(DisplayName = "Create Cliente With Documento Already Registered Should Throw")]
        public async Task Create_WithDocumentoJaCadastrado_ShouldThrowInvalidDocumentoException()
        {
            var dto = new ClienteCreateDto("João Silva", CpfValido, "11999999999", "joao@teste.com");
            var clienteExistente = new Cliente("Outro Cliente", CpfValido, "11988888888", "outro@teste.com");

            _repository.GetByDocumentoOrEmail(Arg.Any<string>(), Arg.Any<string>()).Returns(clienteExistente);

            Func<Task> action = async () => await _useCase.ExecuteAsync(dto);

            await action.Should().ThrowAsync<InvalidDocumentoException>();
            await _repository.DidNotReceive().Create(Arg.Any<Cliente>());
        }

        [Fact(DisplayName = "Create Cliente With Email Already Registered Should Throw")]
        public async Task Create_WithEmailJaCadastrado_ShouldThrowInvalidEmailException()
        {
            const string outroCpfValido = "11144477735";
            var dto = new ClienteCreateDto("João Silva", CpfValido, "11999999999", "joao@teste.com");
            var clienteExistente = new Cliente("Outro Cliente", outroCpfValido, "11988888888", "joao@teste.com");

            _repository.GetByDocumentoOrEmail(Arg.Any<string>(), Arg.Any<string>()).Returns(clienteExistente);

            Func<Task> action = async () => await _useCase.ExecuteAsync(dto);

            await action.Should().ThrowAsync<InvalidEmailException>();
        }
    }
}
