using AutoReparos.Application.Clientes.UseCases;
using AutoReparos.Application.Veiculos.DTOs.Response;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Clientes;

public class ObterMeusVeiculosUseCaseTests
{
    private readonly IVeiculoRepository _veiculoRepository = Substitute.For<IVeiculoRepository>();
    private readonly ObterMeusVeiculosUseCase _useCase;

    public ObterMeusVeiculosUseCaseTests()
    {
        _useCase = new ObterMeusVeiculosUseCase(_veiculoRepository);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarListaVazia_QuandoClienteIdForVazio()
    {
        // Act
        var result = await _useCase.ExecuteAsync(Guid.Empty);

        // Assert
        result.Should().BeEmpty();
        await _veiculoRepository.DidNotReceiveWithAnyArgs().GetAll(default, default, default);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarVeiculosDoCliente_QuandoClientePossuirVeiculos()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculo1 = new Veiculo(clienteId, "Volkswagen", "Gol", 2020, 2021, new Placa("ABC1D23"), new Chassi("9BWZZZ377VT004251"), new Renavam("00123456789"));
        var veiculo2 = new Veiculo(clienteId, "Fiat", "Palio", 2018, 2019, new Placa("XYZ2E34"), new Chassi("9BWZZZ377VT004252"), new Renavam("00123456788"));

        _veiculoRepository.GetAll(clienteId, 0, 100)
            .Returns(((IEnumerable<Veiculo>)new[] { veiculo1, veiculo2 }, 2));

        // Act
        var result = await _useCase.ExecuteAsync(clienteId);

        // Assert
        result.Should().HaveCount(2);
        result.Select(v => v.Placa).Should().Contain(new[] { "ABC1D23", "XYZ2E34" });
        result.All(v => v.ClienteId == clienteId).Should().BeTrue();
    }
}
