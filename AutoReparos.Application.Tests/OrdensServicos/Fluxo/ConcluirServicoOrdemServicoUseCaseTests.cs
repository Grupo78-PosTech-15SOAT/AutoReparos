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
    public class ConcluirServicoOrdemServicoUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly IClienteRepository _clienteRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly ILogger<ConcluirServicoOrdemServicoUseCase> _logger;
        private readonly ConcluirServicoOrdemServicoUseCase _useCase;

        public ConcluirServicoOrdemServicoUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _clienteRepository = Substitute.For<IClienteRepository>();
            _notificacaoService = Substitute.For<INotificacaoService>();
            _logger = Substitute.For<ILogger<ConcluirServicoOrdemServicoUseCase>>();
            _useCase = new ConcluirServicoOrdemServicoUseCase(_repository, _clienteRepository, _notificacaoService, _logger);
        }

        [Fact(DisplayName = "ConcluirServico When Last Item Should Finalize OrdemServico And Notify")]
        public async Task ConcluirServico_WhenLastItem_ShouldFinalizeEnotify()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var os = new OrdemServico(cliente.Id, Guid.NewGuid(), "obs");
            var item = new OrdemServicoServico(os.Id, Guid.NewGuid(), 150m);
            os.AdicionarServico(item);
            os.IniciarDiagnostico();
            os.AguardarAprovacao();
            os.Aprovar();
            os.IniciarServico(item.Id);

            _repository.GetById(os.Id).Returns(os);
            _clienteRepository.GetById(cliente.Id).Returns(cliente);

            await _useCase.ExecuteAsync(os.Id, item.Id);

            item.Status.Should().Be(EStatusServicoOS.Concluido);
            os.Status.Should().Be(EStatusOrdemServico.Finalizada);
            await _repository.Received(1).Update(os);
            await _notificacaoService.Received(1).EnviarAtualizacaoStatus(
                cliente.Email.Endereco, cliente.Nome, os.Id, "EmExecucao", "Finalizada");
        }

        [Fact(DisplayName = "ConcluirServico When OrdemServico Does Not Exist Should Throw NotFoundException")]
        public async Task ConcluirServico_WhenOrdemServicoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((OrdemServico?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id, Guid.NewGuid());

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "ConcluirServico When NotificacaoService Throws Should Not Propagate Exception")]
        public async Task ConcluirServico_WhenNotificacaoServiceThrows_ShouldNotPropagateException()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var os = new OrdemServico(cliente.Id, Guid.NewGuid(), "obs");
            var item = new OrdemServicoServico(os.Id, Guid.NewGuid(), 150m);
            os.AdicionarServico(item);
            os.IniciarDiagnostico();
            os.AguardarAprovacao();
            os.Aprovar();
            os.IniciarServico(item.Id);

            _repository.GetById(os.Id).Returns(os);
            _clienteRepository.GetById(cliente.Id).Returns(cliente);
            _notificacaoService
                .EnviarAtualizacaoStatus(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
                .ThrowsAsync(new InvalidOperationException("Falha no envio de e-mail"));

            Func<Task> action = async () => await _useCase.ExecuteAsync(os.Id, item.Id);

            await action.Should().NotThrowAsync();
            os.Status.Should().Be(EStatusOrdemServico.Finalizada);
            await _repository.Received(1).Update(os);
        }
    }
}
