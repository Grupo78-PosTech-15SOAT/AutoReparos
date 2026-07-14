using AutoReparos.Application.OrdensServicos.UseCases.Core;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.OrdensServicos.Core
{
    public class ObterOrdemServicoPublicaPorIdUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly ObterOrdemServicoPublicaPorIdUseCase _useCase;

        public ObterOrdemServicoPublicaPorIdUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _useCase = new ObterOrdemServicoPublicaPorIdUseCase(_repository);
        }

        [Fact(DisplayName = "GetPublicById When OrdemServico Exists Should Return Dto")]
        public async Task GetPublicById_WhenOrdemServicoExists_ShouldReturnDto()
        {
            var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
            _repository.GetById(os.Id).Returns(os);

            var result = await _useCase.ExecuteAsync(os.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(os.Id);
        }

        [Fact(DisplayName = "GetPublicById When OrdemServico Has Servicos And Insumos Should Return Dto With Items")]
        public async Task GetPublicById_WhenOrdemServicoHasServicosEInsumos_ShouldReturnDtoComItens()
        {
            var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
            var servico = new OrdemServicoServico(os.Id, Guid.NewGuid(), 150m);
            var insumo = new OrdemServicoInsumo(os.Id, Guid.NewGuid(), "Óleo 5W30", 45m, 2, EOrigemInsumo.Estoque);
            os.AdicionarServico(servico);
            os.AdicionarInsumo(insumo);

            _repository.GetById(os.Id).Returns(os);

            var result = await _useCase.ExecuteAsync(os.Id);

            result.Should().NotBeNull();
            result!.Servicos.Should().ContainSingle(s => s.Id == servico.Id);
            result.Insumos.Should().ContainSingle(i => i.Id == insumo.Id && i.Descricao == insumo.Descricao && i.Quantidade == insumo.Quantidade);
        }

        [Fact(DisplayName = "GetPublicById When OrdemServico Does Not Exist Should Return Null")]
        public async Task GetPublicById_WhenOrdemServicoDoesNotExist_ShouldReturnNull()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((OrdemServico?)null);

            var result = await _useCase.ExecuteAsync(id);

            result.Should().BeNull();
        }
    }
}
