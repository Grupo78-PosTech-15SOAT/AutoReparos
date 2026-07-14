using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.UseCases;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Veiculos
{
    public class CriarVeiculoUseCaseTests
    {
        private readonly IVeiculoRepository _repository;
        private readonly IClienteRepository _clienteRepository;
        private readonly CriarVeiculoUseCase _useCase;

        public CriarVeiculoUseCaseTests()
        {
            _repository = Substitute.For<IVeiculoRepository>();
            _clienteRepository = Substitute.For<IClienteRepository>();
            _useCase = new CriarVeiculoUseCase(_repository, _clienteRepository);
        }

        [Fact(DisplayName = "Create Veiculo With Valid Data Should Return Dto")]
        public async Task Create_WithValidData_ShouldReturnDto()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var dto = new VeiculoCreateDto(cliente.Id, "Chevrolet", "Onix", 2020, 2021, "ABC1D23", "9BD111060T5002156", "00123456789");

            _clienteRepository.GetById(cliente.Id).Returns(cliente);

            var result = await _useCase.ExecuteAsync(dto);

            result.Should().NotBeNull();
            result.Marca.Should().Be(dto.Marca);
            result.Placa.Should().Be("ABC1D23");
            await _repository.Received(1).Create(Arg.Any<Veiculo>());
        }

        [Fact(DisplayName = "Create Veiculo With Cliente Not Found Should Throw NotFoundException")]
        public async Task Create_WithClienteNotFound_ShouldThrowNotFoundException()
        {
            var dto = new VeiculoCreateDto(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, "ABC1D23", "9BD111060T5002156", "00123456789");
            _clienteRepository.GetById(dto.ClienteId).Returns((Cliente?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(dto);

            await action.Should().ThrowAsync<NotFoundException>();
            await _repository.DidNotReceive().Create(Arg.Any<Veiculo>());
        }
    }
}
