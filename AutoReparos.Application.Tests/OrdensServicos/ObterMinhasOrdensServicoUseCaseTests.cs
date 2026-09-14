using AutoReparos.Application.OrdensServicos.UseCases;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.OrdensServicos;

public class ObterMinhasOrdensServicoUseCaseTests
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = Substitute.For<IOrdemServicoRepository>();
    private readonly IVeiculoRepository _veiculoRepository = Substitute.For<IVeiculoRepository>();
    private readonly ObterMinhasOrdensServicoUseCase _useCase;

    public ObterMinhasOrdensServicoUseCaseTests()
    {
        _useCase = new ObterMinhasOrdensServicoUseCase(_ordemServicoRepository, _veiculoRepository);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarVazio_QuandoClienteIdForVazio()
    {
        // Act
        var result = await _useCase.ExecuteAsync(Guid.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarVazio_QuandoPlacaInformadaNaoExistir()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        _veiculoRepository.GetByPlaca("INEXISTENTE").Returns((Veiculo?)null);

        // Act
        var result = await _useCase.ExecuteAsync(clienteId, "INEXISTENTE");

        // Assert
        result.Should().BeEmpty();
        await _ordemServicoRepository.DidNotReceiveWithAnyArgs().GetAll(default, default, default, default, default);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarVazio_QuandoPlacaPertencerAOutroCliente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var outroClienteId = Guid.NewGuid();
        var veiculoDeOutro = new Veiculo(outroClienteId, "Toyota", "Corolla", 2022, 2023, new Placa("XYZ9999"), new Chassi("9BWZZZ377VT004999"), new Renavam("00123456999"));

        _veiculoRepository.GetByPlaca("XYZ9999").Returns(veiculoDeOutro);

        // Act
        var result = await _useCase.ExecuteAsync(clienteId, "XYZ9999");

        // Assert
        result.Should().BeEmpty();
        await _ordemServicoRepository.DidNotReceiveWithAnyArgs().GetAll(default, default, default, default, default);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarOrdensDoCliente_QuandoPlacaPertencerAoCliente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculo = new Veiculo(clienteId, "Volkswagen", "Gol", 2020, 2021, new Placa("ABC1D23"), new Chassi("9BWZZZ377VT004251"), new Renavam("00123456789"));
        var os = new OrdemServico(clienteId, veiculo.Id, "Barulho na suspensão");

        _veiculoRepository.GetByPlaca("ABC1D23").Returns(veiculo);
        _veiculoRepository.GetByIds(Arg.Any<IEnumerable<Guid>>()).Returns(new[] { veiculo });
        _ordemServicoRepository.GetAll(clienteId, veiculo.Id, null, 0, 100)
            .Returns(((IEnumerable<OrdemServico>)new[] { os }, 1));

        // Act
        var result = await _useCase.ExecuteAsync(clienteId, "ABC1D23");

        // Assert
        result.Should().HaveCount(1);
        result.First().VeiculoId.Should().Be(veiculo.Id);
        result.First().PlacaVeiculo.Should().Be("ABC1D23");
        result.First().ModeloVeiculo.Should().Be("Gol");
        result.First().Observacao.Should().Be("Barulho na suspensão");
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarTodasOrdensDoCliente_QuandoPlacaNaoForInformada()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var veiculoId1 = Guid.NewGuid();
        var veiculoId2 = Guid.NewGuid();
        var os1 = new OrdemServico(clienteId, veiculoId1, "Revisão geral");
        var os2 = new OrdemServico(clienteId, veiculoId2, "Troca de óleo");

        _veiculoRepository.GetByIds(Arg.Any<IEnumerable<Guid>>()).Returns(Enumerable.Empty<Veiculo>());
        _ordemServicoRepository.GetAll(clienteId, null, null, 0, 100)
            .Returns(((IEnumerable<OrdemServico>)new[] { os1, os2 }, 2));

        // Act
        var result = await _useCase.ExecuteAsync(clienteId);

        // Assert
        result.Should().HaveCount(2);
        await _veiculoRepository.DidNotReceiveWithAnyArgs().GetByPlaca(default!);
    }
}
