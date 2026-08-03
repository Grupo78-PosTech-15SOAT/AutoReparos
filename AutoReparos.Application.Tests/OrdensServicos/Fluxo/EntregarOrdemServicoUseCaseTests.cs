using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Fluxo;
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

namespace AutoReparos.Application.Tests.OrdensServicos.Fluxo
{
    public class EntregarOrdemServicoUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly IClienteRepository _clienteRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly ILogger<EntregarOrdemServicoUseCase> _logger;
        private readonly EntregarOrdemServicoUseCase _useCase;

        public EntregarOrdemServicoUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _clienteRepository = Substitute.For<IClienteRepository>();
            _notificacaoService = Substitute.For<INotificacaoService>();
            _logger = Substitute.For<ILogger<EntregarOrdemServicoUseCase>>();
            _useCase = new EntregarOrdemServicoUseCase(_repository, _clienteRepository, _notificacaoService, _logger);
        }

        private static OrdemServico CriarOrdemFinalizada(Guid clienteId)
        {
            var os = new OrdemServico(clienteId, Guid.NewGuid(), "obs");
            var item = new OrdemServicoServico(os.Id, Guid.NewGuid(), 150m);
            os.AdicionarServico(item);
            os.IniciarDiagnostico("mecanico-123");
            os.AguardarAprovacao("mecanico-123");
            os.Aprovar();
            os.IniciarServico(item.Id);
            os.ConcluirServico(item.Id);
            return os;
        }

        [Fact(DisplayName = "Entregar When OrdemServico Finalizada Should Change Status And Notify")]
        public async Task Entregar_WhenFinalizada_ShouldChangeStatusENotify()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var os = CriarOrdemFinalizada(cliente.Id);

            _repository.GetById(os.Id).Returns(os);
            _clienteRepository.GetById(cliente.Id).Returns(cliente);

            await _useCase.ExecuteAsync(os.Id);

            os.Status.Should().Be(EStatusOrdemServico.Entregue);
            os.EntregueEm.Should().NotBeNull();
            await _repository.Received(1).Update(os);
        }

        [Fact(DisplayName = "Entregar When OrdemServico Does Not Exist Should Throw NotFoundException")]
        public async Task Entregar_WhenOrdemServicoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((OrdemServico?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id);

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "Entregar When NotificacaoService Throws Should Not Propagate Exception")]
        public async Task Entregar_WhenNotificacaoServiceThrows_ShouldNotPropagateException()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var os = CriarOrdemFinalizada(cliente.Id);

            _repository.GetById(os.Id).Returns(os);
            _clienteRepository.GetById(cliente.Id).Returns(cliente);
            _notificacaoService
                .EnviarAtualizacaoStatus(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
                .ThrowsAsync(new InvalidOperationException("Falha no envio de e-mail"));

            Func<Task> action = async () => await _useCase.ExecuteAsync(os.Id);

            await action.Should().NotThrowAsync();
            os.Status.Should().Be(EStatusOrdemServico.Entregue);
            await _repository.Received(1).Update(os);
        }
    }
}
