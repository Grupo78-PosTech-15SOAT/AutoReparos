using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Fluxo;
using AutoReparos.Application.Shared.Interfaces;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AutoReparos.Application.Tests.OrdensServicos.Fluxo
{
    public class EnviarOrdemServicoParaAprovacaoUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IAprovacaoTokenService _aprovacaoTokenService;
        private readonly IClienteRepository _clienteRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly ILogger<EnviarOrdemServicoParaAprovacaoUseCase> _logger;
        private readonly EnviarOrdemServicoParaAprovacaoUseCase _useCase;

        public EnviarOrdemServicoParaAprovacaoUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _servicoRepository = Substitute.For<IServicoRepository>();
            _aprovacaoTokenService = Substitute.For<IAprovacaoTokenService>();
            _clienteRepository = Substitute.For<IClienteRepository>();
            _notificacaoService = Substitute.For<INotificacaoService>();
            _logger = Substitute.For<ILogger<EnviarOrdemServicoParaAprovacaoUseCase>>();
            _useCase = new EnviarOrdemServicoParaAprovacaoUseCase(
                _repository, _servicoRepository, _aprovacaoTokenService, _clienteRepository, _notificacaoService, _logger);
        }

        [Fact(DisplayName = "Enviar When OrdemServico Em Diagnostico Com Servico Should Change Status And Send Orcamento")]
        public async Task Enviar_WhenEmDiagnosticoComServico_ShouldChangeStatusESendOrcamento()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var servico = new Servico("Troca de óleo", "Descrição", 150m);
            var os = new OrdemServico(cliente.Id, Guid.NewGuid(), "obs");
            os.AdicionarServico(new OrdemServicoServico(os.Id, servico.Id, 150m));
            os.IniciarDiagnostico();

            _repository.GetById(os.Id).Returns(os);
            _servicoRepository.GetById(servico.Id).Returns(servico);
            _clienteRepository.GetById(cliente.Id).Returns(cliente);
            _aprovacaoTokenService.GerarToken(os.Id).Returns("token-123");

            await _useCase.ExecuteAsync(os.Id);

            os.Status.Should().Be(EStatusOrdemServico.AguardandoAprovacao);
            await _repository.Received(1).Update(os);
            await _notificacaoService.Received(1).EnviarOrcamento(
                cliente.Email.Endereco, cliente.Nome, "token-123", os.ValorTotal, Arg.Any<IEnumerable<Application.Servicos.DTOs.Response.ServicoDto>>());
        }

        [Fact(DisplayName = "Enviar When OrdemServico Does Not Exist Should Throw NotFoundException")]
        public async Task Enviar_WhenOrdemServicoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((OrdemServico?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id);

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "Enviar When NotificacaoService Throws Should Not Propagate Exception")]
        public async Task Enviar_WhenNotificacaoServiceThrows_ShouldNotPropagateException()
        {
            var cliente = new Cliente("João Silva", "52998224725", "11999999999", "joao@teste.com");
            var servico = new Servico("Troca de óleo", "Descrição", 150m);
            var os = new OrdemServico(cliente.Id, Guid.NewGuid(), "obs");
            os.AdicionarServico(new OrdemServicoServico(os.Id, servico.Id, 150m));
            os.IniciarDiagnostico();

            _repository.GetById(os.Id).Returns(os);
            _servicoRepository.GetById(servico.Id).Returns(servico);
            _clienteRepository.GetById(cliente.Id).Returns(cliente);
            _aprovacaoTokenService.GerarToken(os.Id).Returns("token-123");
            _notificacaoService
                .EnviarOrcamento(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<IEnumerable<Application.Servicos.DTOs.Response.ServicoDto>>())
                .ThrowsAsync(new InvalidOperationException("Falha no envio de e-mail"));

            Func<Task> action = async () => await _useCase.ExecuteAsync(os.Id);

            await action.Should().NotThrowAsync();
            os.Status.Should().Be(EStatusOrdemServico.AguardandoAprovacao);
            await _repository.Received(1).Update(os);
        }
    }
}
