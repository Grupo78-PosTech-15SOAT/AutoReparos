using AutoReparos.Application.OrdensServicos.UseCases.Core;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.OrdensServicos.Core
{
    public class ObterOrdemServicoPorIdUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly ObterOrdemServicoPorIdUseCase _useCase;

        public ObterOrdemServicoPorIdUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _useCase = new ObterOrdemServicoPorIdUseCase(_repository);
        }

        [Fact(DisplayName = "GetById When OrdemServico Exists Should Return Dto")]
        public async Task GetById_WhenOrdemServicoExists_ShouldReturnDto()
        {
            var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
            _repository.GetById(os.Id).Returns(os);

            var result = await _useCase.ExecuteAsync(os.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(os.Id);
        }

        [Fact(DisplayName = "GetById When OrdemServico Has Servicos And Insumos Should Return Dto With Items")]
        public async Task GetById_WhenOrdemServicoHasServicosEInsumos_ShouldReturnDtoComItens()
        {
            var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
            var servico = new OrdemServicoServico(os.Id, Guid.NewGuid(), 150m);
            var insumo = new OrdemServicoInsumo(os.Id, Guid.NewGuid(), "Óleo 5W30", 45m, 2, EOrigemInsumo.Estoque);
            os.AdicionarServico(servico);
            os.AdicionarInsumo(insumo);

            _repository.GetById(os.Id).Returns(os);

            var result = await _useCase.ExecuteAsync(os.Id);

            result.Should().NotBeNull();
            result!.Servicos.Should().ContainSingle(s => s.Id == servico.Id && s.ServicoId == servico.ServicoId && s.ValorCobrado == servico.ValorCobrado);
            result.Insumos.Should().ContainSingle(i => i.Id == insumo.Id && i.InsumoId == insumo.InsumoId && i.ValorTotal == insumo.ValorTotal);
        }

        [Fact(DisplayName = "GetById When OrdemServico Does Not Exist Should Return Null")]
        public async Task GetById_WhenOrdemServicoDoesNotExist_ShouldReturnNull()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((OrdemServico?)null);

            var result = await _useCase.ExecuteAsync(id);

            result.Should().BeNull();
        }
    }
}
