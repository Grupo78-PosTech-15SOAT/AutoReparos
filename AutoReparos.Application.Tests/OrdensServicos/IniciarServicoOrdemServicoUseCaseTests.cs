using AutoReparos.Application.OrdensServicos.UseCases;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.OrdensServicos
{
    public class IniciarServicoOrdemServicoUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly IniciarServicoOrdemServicoUseCase _useCase;

        public IniciarServicoOrdemServicoUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _useCase = new IniciarServicoOrdemServicoUseCase(_repository);
        }

        private static OrdemServico CriarOrdemEmExecucao(out Guid ordemServicoServicoId)
        {
            var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
            var item = new OrdemServicoServico(os.Id, Guid.NewGuid(), 150m);
            os.AdicionarServico(item);
            os.IniciarDiagnostico();
            os.AguardarAprovacao();
            os.Aprovar();
            ordemServicoServicoId = item.Id;
            return os;
        }

        [Fact(DisplayName = "IniciarServico When OrdemServico Em Execucao Should Start Item")]
        public async Task IniciarServico_WhenEmExecucao_ShouldStartItem()
        {
            var os = CriarOrdemEmExecucao(out var itemId);
            _repository.GetById(os.Id).Returns(os);

            await _useCase.ExecuteAsync(os.Id, itemId);

            os.Servicos.Single().Status.Should().Be(EStatusServicoOS.EmExecucao);
            await _repository.Received(1).Update(os);
        }

        [Fact(DisplayName = "IniciarServico When OrdemServico Does Not Exist Should Throw NotFoundException")]
        public async Task IniciarServico_WhenOrdemServicoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((OrdemServico?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id, Guid.NewGuid());

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
