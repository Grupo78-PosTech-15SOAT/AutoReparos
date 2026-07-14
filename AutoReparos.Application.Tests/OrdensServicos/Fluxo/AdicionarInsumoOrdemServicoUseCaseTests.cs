using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.UseCases.Fluxo;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;
using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.Tests.OrdensServicos.Fluxo
{
    public class AdicionarInsumoOrdemServicoUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly IInsumoRepository _insumoRepository;
        private readonly AdicionarInsumoOrdemServicoUseCase _useCase;

        public AdicionarInsumoOrdemServicoUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _insumoRepository = Substitute.For<IInsumoRepository>();
            _useCase = new AdicionarInsumoOrdemServicoUseCase(_repository, _insumoRepository);
        }

        [Fact(DisplayName = "AdicionarInsumo With Origem Estoque Should Remove Estoque And Add Item")]
        public async Task AdicionarInsumo_WithOrigemEstoque_ShouldRemoverEstoqueEAddItem()
        {
            var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
            var insumo = new Insumo("Óleo 5W30", "Óleo sintético", 45m, 10);
            var dto = new AdicionarInsumoDto(insumo.Id, "Óleo 5W30", null, 2, EOrigemInsumo.Estoque);

            _repository.GetById(os.Id).Returns(os);
            _insumoRepository.GetById(insumo.Id).Returns(insumo);

            await _useCase.ExecuteAsync(os.Id, dto);

            insumo.QuantidadeEstoque.Should().Be(8);
            os.Insumos.Should().ContainSingle(i => i.InsumoId == insumo.Id);
            await _insumoRepository.Received(1).Update(insumo);
            await _repository.Received(1).Update(os);
        }

        [Fact(DisplayName = "AdicionarInsumo With Insumo Not Found Should Throw NotFoundException")]
        public async Task AdicionarInsumo_WithInsumoNotFound_ShouldThrowNotFoundException()
        {
            var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
            var insumoId = Guid.NewGuid();
            var dto = new AdicionarInsumoDto(insumoId, "Óleo 5W30", null, 2, EOrigemInsumo.Estoque);

            _repository.GetById(os.Id).Returns(os);
            _insumoRepository.GetById(insumoId).Returns((Insumo?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(os.Id, dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "AdicionarInsumo With Compra Especifica Sem Valor Unitario Should Throw ValidationException")]
        public async Task AdicionarInsumo_WithCompraEspecificaSemValorUnitario_ShouldThrowValidationException()
        {
            var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
            var dto = new AdicionarInsumoDto(null, "Peça externa", null, 1, EOrigemInsumo.CompraEspecifica);

            _repository.GetById(os.Id).Returns(os);

            Func<Task> action = async () => await _useCase.ExecuteAsync(os.Id, dto);

            await action.Should().ThrowAsync<ValidationException>();
        }

        [Fact(DisplayName = "AdicionarInsumo With Origem CompraEspecifica And ValorUnitario Should Add Item")]
        public async Task AdicionarInsumo_WithOrigemCompraEspecificaEValorUnitario_ShouldAddItem()
        {
            var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
            var dto = new AdicionarInsumoDto(null, "Peça externa", 80m, 1, EOrigemInsumo.CompraEspecifica);

            _repository.GetById(os.Id).Returns(os);

            await _useCase.ExecuteAsync(os.Id, dto);

            os.Insumos.Should().ContainSingle(i => i.ValorUnitario == 80m && i.Origem == EOrigemInsumo.CompraEspecifica);
            await _repository.Received(1).Update(os);
        }

        [Fact(DisplayName = "AdicionarInsumo When OrdemServico Does Not Exist Should Throw NotFoundException")]
        public async Task AdicionarInsumo_WhenOrdemServicoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            var dto = new AdicionarInsumoDto(Guid.NewGuid(), "Óleo 5W30", null, 2, EOrigemInsumo.Estoque);
            _repository.GetById(id).Returns((OrdemServico?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id, dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
