using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Aprovacao;
using AutoReparos.Application.Shared.Interfaces;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AutoReparos.Application.Tests.OrdensServicos.Aprovacao
{
    public class RecusarOrdemServicoUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly IAprovacaoTokenService _aprovacaoTokenService;
        private readonly IClienteRepository _clienteRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly ILogger<RecusarOrdemServicoUseCase> _logger;
        private readonly RecusarOrdemServicoUseCase _useCase;

        public RecusarOrdemServicoUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _aprovacaoTokenService = Substitute.For<IAprovacaoTokenService>();
            _clienteRepository = Substitute.For<IClienteRepository>();
            _notificacaoService = Substitute.For<INotificacaoService>();
            _logger = Substitute.For<ILogger<RecusarOrdemServicoUseCase>>();
            _useCase = new RecusarOrdemServicoUseCase(_repository, _aprovacaoTokenService, _clienteRepository, _notificacaoService, _logger);
        }

        [Fact(DisplayName = "Recusar With Valid Token Should Change Status Back To EmDiagnostico")]
        public async Task Recusar_WithValidToken_ShouldChangeStatusBackToEmDiagnostico()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var os = new OrdemServico(cliente.Id, Guid.NewGuid(), "obs");
            os.AdicionarServico(new OrdemServicoServico(os.Id, Guid.NewGuid(), 150m));
            os.IniciarDiagnostico();
            os.AguardarAprovacao();

            _aprovacaoTokenService.ValidarToken("token-123").Returns(os.Id);
            _repository.GetById(os.Id).Returns(os);
            _clienteRepository.GetById(cliente.Id).Returns(cliente);

            await _useCase.ExecuteAsync("token-123");

            os.Status.Should().Be(EStatusOrdemServico.EmDiagnostico);
            await _repository.Received(1).Update(os);
        }

        [Fact(DisplayName = "Recusar When OrdemServico Does Not Exist Should Throw NotFoundException")]
        public async Task Recusar_WhenOrdemServicoDoesNotExist_ShouldThrowNotFoundException()
        {
            var ordemServicoId = Guid.NewGuid();
            _aprovacaoTokenService.ValidarToken("token-invalido").Returns(ordemServicoId);
            _repository.GetById(ordemServicoId).Returns((OrdemServico?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync("token-invalido");

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "Recusar When NotificacaoService Throws Should Not Propagate Exception")]
        public async Task Recusar_WhenNotificacaoServiceThrows_ShouldNotPropagateException()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var os = new OrdemServico(cliente.Id, Guid.NewGuid(), "obs");
            os.AdicionarServico(new OrdemServicoServico(os.Id, Guid.NewGuid(), 150m));
            os.IniciarDiagnostico();
            os.AguardarAprovacao();

            _aprovacaoTokenService.ValidarToken("token-123").Returns(os.Id);
            _repository.GetById(os.Id).Returns(os);
            _clienteRepository.GetById(cliente.Id).Returns(cliente);
            _notificacaoService
                .EnviarAtualizacaoStatus(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
                .ThrowsAsync(new InvalidOperationException("Falha no envio de e-mail"));

            Func<Task> action = async () => await _useCase.ExecuteAsync("token-123");

            await action.Should().NotThrowAsync();
            os.Status.Should().Be(EStatusOrdemServico.EmDiagnostico);
            await _repository.Received(1).Update(os);
        }
    }
}
