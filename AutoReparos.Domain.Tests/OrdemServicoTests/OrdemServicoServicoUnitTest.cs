using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Exceptions;
using FluentAssertions;

namespace AutoReparos.Domain.Tests.OrdemServicoTests
{
    public class OrdemServicoServicoUnitTest
    {
        private readonly Guid _osId = Guid.NewGuid();
        private readonly Guid _servicoBaseId = Guid.NewGuid();

        [Fact(DisplayName = "Create Valid OS Service")]
        public void CreateServico_WithValidData_ShouldInitializeCorrectly()
        {
            var valor = 250.00m;

            var servico = new OrdemServicoServico(_osId, _servicoBaseId, valor);

            servico.OrdemServicoId.Should().Be(_osId);
            servico.ServicoId.Should().Be(_servicoBaseId);
            servico.ValorCobrado.Should().Be(valor);
            servico.Status.Should().Be(EStatusServicoOS.Pendente);
            servico.IniciadoEm.Should().BeNull();
            servico.ConcluidoEm.Should().BeNull();
        }

        [Theory(DisplayName = "Create Service With Invalid Value")]
        [InlineData(0)]
        [InlineData(-50)]
        public void CreateServico_WithInvalidValue_ShouldThrowException(decimal valorInvalido)
        {
            Action action = () => new OrdemServicoServico(_osId, _servicoBaseId, valorInvalido);

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Valor cobrado deve ser maior que zero.");
        }

        [Fact(DisplayName = "Start Service Successfully")]
        public void Iniciar_WhenPendente_ShouldSetStartDateAndStatus()
        {
            var servico = new OrdemServicoServico(_osId, _servicoBaseId, 100);

            servico.Iniciar();

            servico.Status.Should().Be(EStatusServicoOS.EmExecucao);
            servico.IniciadoEm.Should().NotBeNull();
            servico.IniciadoEm.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact(DisplayName = "Start Service When Already Started")]
        public void Iniciar_WhenNotPendente_ShouldThrowException()
        {
            var servico = new OrdemServicoServico(_osId, _servicoBaseId, 100);
            servico.Iniciar();

            Action action = () => servico.Iniciar();

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Serviço só pode ser iniciado quando estiver pendente.");
        }

        [Fact(DisplayName = "Conclude Service Successfully")]
        public void Concluir_WhenInExecution_ShouldSetEndDateAndStatus()
        {
            var servico = new OrdemServicoServico(_osId, _servicoBaseId, 100);
            servico.Iniciar();

            servico.Concluir();

            servico.Status.Should().Be(EStatusServicoOS.Concluido);
            servico.ConcluidoEm.Should().NotBeNull();
            servico.ConcluidoEm.Value.Should().BeOnOrAfter(servico.IniciadoEm!.Value);
        }

        [Fact(DisplayName = "Conclude Service Without Starting")]
        public void Concluir_WhenPendente_ShouldThrowException()
        {
            var servico = new OrdemServicoServico(_osId, _servicoBaseId, 100);

            Action action = () => servico.Concluir();

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Serviço só pode ser concluído quando estiver em execução.");
        }

        [Fact(DisplayName = "Calculate Execution Time Correctly")]
        public void TempoExecucao_WhenConcluded_ShouldReturnDifference()
        {
            var servico = new OrdemServicoServico(_osId, _servicoBaseId, 100);

            servico.Iniciar();
            servico.Concluir();

            servico.TempoExecucao.Should().NotBeNull();
            servico.TempoExecucao.Value.TotalSeconds.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact(DisplayName = "Execution Time Should Be Null If Not Finished")]
        public void TempoExecucao_WhenNotConcluded_ShouldReturnNull()
        {
            var servico = new OrdemServicoServico(_osId, _servicoBaseId, 100);
            servico.Iniciar();

            servico.TempoExecucao.Should().BeNull();
        }
    }
}
