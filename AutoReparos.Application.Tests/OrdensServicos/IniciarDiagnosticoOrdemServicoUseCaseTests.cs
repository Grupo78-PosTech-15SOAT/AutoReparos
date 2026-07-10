using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases;
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

namespace AutoReparos.Application.Tests.OrdensServicos
{
    public class IniciarDiagnosticoOrdemServicoUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly IClienteRepository _clienteRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly ILogger<IniciarDiagnosticoOrdemServicoUseCase> _logger;
        private readonly IniciarDiagnosticoOrdemServicoUseCase _useCase;

        public IniciarDiagnosticoOrdemServicoUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _clienteRepository = Substitute.For<IClienteRepository>();
            _notificacaoService = Substitute.For<INotificacaoService>();
            _logger = Substitute.For<ILogger<IniciarDiagnosticoOrdemServicoUseCase>>();
            _useCase = new IniciarDiagnosticoOrdemServicoUseCase(_repository, _clienteRepository, _notificacaoService, _logger);
        }

        [Fact(DisplayName = "IniciarDiagnostico When OrdemServico Is Recebida Should Change Status And Notify")]
        public async Task IniciarDiagnostico_WhenRecebida_ShouldChangeStatusAndNotify()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var os = new OrdemServico(cliente.Id, Guid.NewGuid(), "obs");

            _repository.GetById(os.Id).Returns(os);
            _clienteRepository.GetById(cliente.Id).Returns(cliente);

            await _useCase.ExecuteAsync(os.Id);

            os.Status.Should().Be(EStatusOrdemServico.EmDiagnostico);
            await _repository.Received(1).Update(os);
            await _notificacaoService.Received(1).EnviarAtualizacaoStatus(
                cliente.Email.Endereco, cliente.Nome, os.Id, "Recebida", "EmDiagnostico");
        }

        [Fact(DisplayName = "IniciarDiagnostico When OrdemServico Does Not Exist Should Throw NotFoundException")]
        public async Task IniciarDiagnostico_WhenOrdemServicoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((OrdemServico?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id);

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "IniciarDiagnostico When NotificacaoService Throws Should Not Propagate Exception")]
        public async Task IniciarDiagnostico_WhenNotificacaoServiceThrows_ShouldNotPropagateException()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var os = new OrdemServico(cliente.Id, Guid.NewGuid(), "obs");

            _repository.GetById(os.Id).Returns(os);
            _clienteRepository.GetById(cliente.Id).Returns(cliente);
            _notificacaoService
                .EnviarAtualizacaoStatus(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
                .ThrowsAsync(new InvalidOperationException("Falha no envio de e-mail"));

            Func<Task> action = async () => await _useCase.ExecuteAsync(os.Id);

            await action.Should().NotThrowAsync();
            os.Status.Should().Be(EStatusOrdemServico.EmDiagnostico);
            await _repository.Received(1).Update(os);
        }
    }
}
