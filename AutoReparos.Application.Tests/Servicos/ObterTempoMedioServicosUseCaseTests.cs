using AutoReparos.Application.Servicos.UseCases;
using AutoReparos.Domain.Servicos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Servicos
{
    public class ObterTempoMedioServicosUseCaseTests
    {
        private readonly IServicoRepository _repository;
        private readonly ObterTempoMedioServicosUseCase _useCase;

        public ObterTempoMedioServicosUseCaseTests()
        {
            _repository = Substitute.For<IServicoRepository>();
            _useCase = new ObterTempoMedioServicosUseCase(_repository);
        }

        [Fact(DisplayName = "GetTempoMedio Should Return Mapped List")]
        public async Task GetTempoMedio_ShouldReturnMappedList()
        {
            var servicoId = Guid.NewGuid();
            var dados = new List<(Guid ServicoId, string NomeServico, TimeSpan TempoMedio, int TotalExecucoes)>
            {
                (servicoId, "Troca de óleo", TimeSpan.FromHours(1), 5)
            };

            _repository.GetTempoMedio().Returns(dados);

            var result = await _useCase.ExecuteAsync();

            result.Should().ContainSingle(r => r.ServicoId == servicoId && r.TotalExecucoes == 5);
        }
    }
}
