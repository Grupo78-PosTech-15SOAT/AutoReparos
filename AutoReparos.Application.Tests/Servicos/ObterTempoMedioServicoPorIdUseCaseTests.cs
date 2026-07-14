using AutoReparos.Application.Servicos.UseCases;
using AutoReparos.Domain.Servicos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Servicos
{
    public class ObterTempoMedioServicoPorIdUseCaseTests
    {
        private readonly IServicoRepository _repository;
        private readonly ObterTempoMedioServicoPorIdUseCase _useCase;

        public ObterTempoMedioServicoPorIdUseCaseTests()
        {
            _repository = Substitute.For<IServicoRepository>();
            _useCase = new ObterTempoMedioServicoPorIdUseCase(_repository);
        }

        [Fact(DisplayName = "GetTempoMedioById When Exists Should Return Dto")]
        public async Task GetTempoMedioById_WhenExists_ShouldReturnDto()
        {
            var servicoId = Guid.NewGuid();
            (Guid ServicoId, string NomeServico, TimeSpan TempoMedio, int TotalExecucoes) dados = (servicoId, "Troca de óleo", TimeSpan.FromHours(1), 5);

            _repository.GetTempoMedioById(servicoId).Returns(dados);

            var result = await _useCase.ExecuteAsync(servicoId);

            result.Should().NotBeNull();
            result!.TotalExecucoes.Should().Be(5);
        }

        [Fact(DisplayName = "GetTempoMedioById When Does Not Exist Should Return Null")]
        public async Task GetTempoMedioById_WhenDoesNotExist_ShouldReturnNull()
        {
            var servicoId = Guid.NewGuid();
            _repository.GetTempoMedioById(servicoId).Returns(((Guid, string, TimeSpan, int)?)null);

            var result = await _useCase.ExecuteAsync(servicoId);

            result.Should().BeNull();
        }
    }
}
