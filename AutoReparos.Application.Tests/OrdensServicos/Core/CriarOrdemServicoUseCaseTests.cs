using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Core;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Exceptions;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.Tests.OrdensServicos.Core
{
    public class CriarOrdemServicoUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly IInsumoRepository _insumoRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly ILogger<CriarOrdemServicoUseCase> _logger;
        private readonly CriarOrdemServicoUseCase _useCase;

        private readonly Cliente _cliente;
        private readonly Veiculo _veiculo;

        public CriarOrdemServicoUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _veiculoRepository = Substitute.For<IVeiculoRepository>();
            _insumoRepository = Substitute.For<IInsumoRepository>();
            _servicoRepository = Substitute.For<IServicoRepository>();
            _clienteRepository = Substitute.For<IClienteRepository>();
            _notificacaoService = Substitute.For<INotificacaoService>();
            _logger = Substitute.For<ILogger<CriarOrdemServicoUseCase>>();
            _useCase = new CriarOrdemServicoUseCase(
                _repository, _veiculoRepository, _insumoRepository, _servicoRepository,
                _clienteRepository, _notificacaoService, _logger);

            _cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            _veiculo = new Veiculo(_cliente.Id, "Chevrolet", "Onix", 2020, 2021, new Placa("ABC1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));

            _veiculoRepository.GetById(_veiculo.Id).Returns(_veiculo);
            _clienteRepository.GetById(_cliente.Id).Returns(_cliente);
        }

        [Fact(DisplayName = "Create With Valid Data Should Return Dto")]
        public async Task Create_WithValidData_ShouldReturnDto()
        {
            var dto = new CriarOrdemServicoDto(_cliente.Id, _veiculo.Id, "Barulho no motor");

            var result = await _useCase.ExecuteAsync(dto);

            result.Should().NotBeNull();
            result.ClienteId.Should().Be(_cliente.Id);
            result.VeiculoId.Should().Be(_veiculo.Id);
            await _repository.Received(1).Create(Arg.Any<OrdemServico>());
        }

        [Fact(DisplayName = "Create With Veiculo Not Found Should Throw NotFoundException")]
        public async Task Create_WithVeiculoNotFound_ShouldThrowNotFoundException()
        {
            var dto = new CriarOrdemServicoDto(_cliente.Id, Guid.NewGuid(), "Barulho no motor");

            Func<Task> action = async () => await _useCase.ExecuteAsync(dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "Create With Veiculo Not Belonging To Cliente Should Throw InvalidVeiculoException")]
        public async Task Create_WithVeiculoNotBelongingToCliente_ShouldThrowInvalidVeiculoException()
        {
            var outroClienteId = Guid.NewGuid();
            var dto = new CriarOrdemServicoDto(outroClienteId, _veiculo.Id, "Barulho no motor");

            Func<Task> action = async () => await _useCase.ExecuteAsync(dto);

            await action.Should().ThrowAsync<InvalidVeiculoException>();
        }

        [Fact(DisplayName = "Create With Cliente Not Found Should Throw NotFoundException")]
        public async Task Create_WithClienteNotFound_ShouldThrowNotFoundException()
        {
            var veiculoSemCliente = new Veiculo(Guid.NewGuid(), "Fiat", "Uno", 2015, 2015, new Placa("XYZ9Z88"), new Chassi("9BD111060T5002199"), new Renavam("00987654321"));
            _veiculoRepository.GetById(veiculoSemCliente.Id).Returns(veiculoSemCliente);

            var dto = new CriarOrdemServicoDto(veiculoSemCliente.ClienteId, veiculoSemCliente.Id, "Barulho no motor");

            Func<Task> action = async () => await _useCase.ExecuteAsync(dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "Create With Servico Should Add Item")]
        public async Task Create_WithServico_ShouldAddItem()
        {
            var servico = new Servico("Troca de óleo", "Descrição", 150m);
            _servicoRepository.GetById(servico.Id).Returns(servico);

            var dto = new CriarOrdemServicoDto(
                _cliente.Id, _veiculo.Id, "Barulho no motor",
                Servicos: [new AdicionarServicoDto(servico.Id, 150m)]);

            var result = await _useCase.ExecuteAsync(dto);

            result.ValorTotal.Should().Be(150m);
        }

        [Fact(DisplayName = "Create With Servico Not Found Should Throw NotFoundException")]
        public async Task Create_WithServicoNotFound_ShouldThrowNotFoundException()
        {
            var servicoId = Guid.NewGuid();
            _servicoRepository.GetById(servicoId).Returns((Servico?)null);

            var dto = new CriarOrdemServicoDto(
                _cliente.Id, _veiculo.Id, "Barulho no motor",
                Servicos: [new AdicionarServicoDto(servicoId, 150m)]);

            Func<Task> action = async () => await _useCase.ExecuteAsync(dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "Create With Insumo Estoque Should Remove Estoque And Add Item")]
        public async Task Create_WithInsumoEstoque_ShouldRemoverEstoqueEAddItem()
        {
            var insumo = new Insumo("Óleo 5W30", "Óleo sintético", 45m, 10);
            _insumoRepository.GetById(insumo.Id).Returns(insumo);

            var dto = new CriarOrdemServicoDto(
                _cliente.Id, _veiculo.Id, "Barulho no motor",
                Insumos: [new AdicionarInsumoDto(insumo.Id, "Óleo 5W30", null, 2, EOrigemInsumo.Estoque)]);

            var result = await _useCase.ExecuteAsync(dto);

            insumo.QuantidadeEstoque.Should().Be(8);
            result.ValorTotal.Should().Be(90m);
            await _insumoRepository.Received(1).Update(insumo);
        }

        [Fact(DisplayName = "Create With Insumo Not Found Should Throw NotFoundException")]
        public async Task Create_WithInsumoNotFound_ShouldThrowNotFoundException()
        {
            var insumoId = Guid.NewGuid();
            _insumoRepository.GetById(insumoId).Returns((Insumo?)null);

            var dto = new CriarOrdemServicoDto(
                _cliente.Id, _veiculo.Id, "Barulho no motor",
                Insumos: [new AdicionarInsumoDto(insumoId, "Óleo 5W30", null, 2, EOrigemInsumo.Estoque)]);

            Func<Task> action = async () => await _useCase.ExecuteAsync(dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "Create With Insumo Compra Especifica Sem Valor Unitario Should Throw ValidationException")]
        public async Task Create_WithInsumoCompraEspecificaSemValorUnitario_ShouldThrowValidationException()
        {
            var dto = new CriarOrdemServicoDto(
                _cliente.Id, _veiculo.Id, "Barulho no motor",
                Insumos: [new AdicionarInsumoDto(null, "Peça externa", null, 1, EOrigemInsumo.CompraEspecifica)]);

            Func<Task> action = async () => await _useCase.ExecuteAsync(dto);

            await action.Should().ThrowAsync<ValidationException>();
        }

        [Fact(DisplayName = "Create With Insumo Compra Especifica Com Valor Unitario Should Add Item")]
        public async Task Create_WithInsumoCompraEspecificaComValorUnitario_ShouldAddItem()
        {
            var dto = new CriarOrdemServicoDto(
                _cliente.Id, _veiculo.Id, "Barulho no motor",
                Insumos: [new AdicionarInsumoDto(null, "Peça externa", 80m, 1, EOrigemInsumo.CompraEspecifica)]);

            var result = await _useCase.ExecuteAsync(dto);

            result.ValorTotal.Should().Be(80m);
        }

        [Fact(DisplayName = "Create With Insumo Estoque Sem InsumoId Should Throw ValidationException")]
        public async Task Create_WithInsumoEstoqueSemInsumoId_ShouldThrowValidationException()
        {
            var dto = new CriarOrdemServicoDto(
                _cliente.Id, _veiculo.Id, "Barulho no motor",
                Insumos: [new AdicionarInsumoDto(null, "Óleo 5W30", null, 2, EOrigemInsumo.Estoque)]);

            Func<Task> action = async () => await _useCase.ExecuteAsync(dto);

            await action.Should().ThrowAsync<ValidationException>();
        }

        [Fact(DisplayName = "Create When NotificacaoService Throws Should Not Propagate Exception")]
        public async Task Create_WhenNotificacaoServiceThrows_ShouldNotPropagateException()
        {
            var dto = new CriarOrdemServicoDto(_cliente.Id, _veiculo.Id, "Barulho no motor");

            _notificacaoService
                .EnviarAtualizacaoStatus(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
                .ThrowsAsync(new InvalidOperationException("Falha no envio de e-mail"));

            Func<Task<Application.OrdensServicos.DTOs.Response.OrdemServicoDto>> action = async () => await _useCase.ExecuteAsync(dto);

            var result = await action.Should().NotThrowAsync();
            result.Subject.Should().NotBeNull();
            await _repository.Received(1).Create(Arg.Any<OrdemServico>());
        }
    }
}
