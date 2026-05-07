using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Exceptions;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;

namespace AutoReparos.Domain.Tests.OrdemServicoTests
{
    public class OrdemServicoUnitTest
    {
        private readonly Guid _clienteIdValido = Guid.NewGuid();
        private readonly Guid _veiculoIdValido = Guid.NewGuid();

        [Fact(DisplayName = "Create OS With Valid Data")]
        public void CreateOS_WithValidData_ShouldInitializeCorrectly()
        {
            var os = new OrdemServico(_clienteIdValido, _veiculoIdValido, "Troca de óleo");

            os.Status.Should().Be(EStatusOrdemServico.Recebida);
            os.ClienteId.Should().Be(_clienteIdValido);
            os.VeiculoId.Should().Be(_veiculoIdValido);
            os.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact(DisplayName = "Create OS With Empty ClienteId")]
        public void CreateOS_WithEmptyClienteId_ShouldThrowException()
        {
            Action action = () => new OrdemServico(Guid.Empty, _veiculoIdValido, "Obs");
            action.Should().Throw<InvalidOrdemServicoException>().WithMessage("Cliente é obrigatório.");
        }

        [Fact(DisplayName = "Add Service In Valid Status")]
        public void AddService_WhenStatusIsRecebida_ShouldSuccess()
        {
            var os = new OrdemServico(_clienteIdValido, _veiculoIdValido, null);
            var servico = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), 150.00m);

            os.AdicionarServico(servico);

            os.Servicos.Should().HaveCount(1);
            os.ValorTotal.Should().Be(150.00m);
        }

        [Fact(DisplayName = "Add Service In Invalid Status")]
        public void AddService_WhenStatusIsAguardandoAprovacao_ShouldThrowException()
        {
            var os = new OrdemServico(_clienteIdValido, _veiculoIdValido, null);
            os.AdicionarServico(new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), 100));
            os.IniciarDiagnostico();
            os.AguardarAprovacao();

            Action action = () => os.AdicionarServico(new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), 100));

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Serviços só podem ser adicionados quando a Ordem de Serviço estiver recebida ou em diagnóstico.");
        }

        [Fact(DisplayName = "Start Diagnosis Successfully")]
        public void IniciarDiagnostico_WhenStatusIsRecebida_ShouldChangeStatus()
        {
            var os = new OrdemServico(_clienteIdValido, _veiculoIdValido, null);

            os.IniciarDiagnostico();

            os.Status.Should().Be(EStatusOrdemServico.EmDiagnostico);
        }

        [Fact(DisplayName = "Wait For Approval Without Services")]
        public void AguardarAprovacao_WithoutServices_ShouldThrowException()
        {
            var os = new OrdemServico(_clienteIdValido, _veiculoIdValido, null);
            os.IniciarDiagnostico();

            Action action = () => os.AguardarAprovacao();

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("A Ordem de Serviço deve ter pelo menos um serviço para aguardar aprovação.");
        }

        [Fact(DisplayName = "Approve OS Should Set Start Date")]
        public void Aprovar_WhenStatusIsAwaiting_ShouldSetIniciadoEm()
        {
            var os = new OrdemServico(_clienteIdValido, _veiculoIdValido, null);
            os.AdicionarServico(new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), 100));
            os.IniciarDiagnostico();
            os.AguardarAprovacao();

            os.Aprovar();

            os.Status.Should().Be(EStatusOrdemServico.EmExecucao);
            os.IniciadoEm.Should().NotBeNull();
        }

        [Fact(DisplayName = "Conclude Service And Finalize OS")]
        public void ConcluirServico_WhenAllServicesDone_ShouldFinalizeOS()
        {
            var os = new OrdemServico(_clienteIdValido, _veiculoIdValido, null);
            var servicoId = Guid.NewGuid();
            var servico = new OrdemServicoServico(servicoId, Guid.NewGuid(), 200);
            servico.Iniciar();

            os.AdicionarServico(servico);
            os.IniciarDiagnostico();
            os.AguardarAprovacao();
            os.Aprovar();

            os.ConcluirServico(servico.Id);

            os.Status.Should().Be(EStatusOrdemServico.Finalizada);
            os.FinalizadoEm.Should().NotBeNull();
        }

        [Fact(DisplayName = "Conclude Nonexistent Service")]
        public void ConcluirServico_WhenServiceNotFound_ShouldThrowNotFoundException()
        {
            var os = new OrdemServico(_clienteIdValido, _veiculoIdValido, null);
            os.AdicionarServico(new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), 100));
            os.IniciarDiagnostico();
            os.AguardarAprovacao();
            os.Aprovar();

            Action action = () => os.ConcluirServico(Guid.NewGuid());

            action.Should().Throw<NotFoundException>().WithMessage("Serviço não encontrado na Ordem de Serviço.");
        }

        [Fact(DisplayName = "Deliver OS Successfully")]
        public void Entregar_WhenFinalizada_ShouldSetEntregueEm()
        {
            var os = new OrdemServico(_clienteIdValido, _veiculoIdValido, null);
            var servico = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), 100);
            servico.Iniciar();
            os.AdicionarServico(servico);
            os.IniciarDiagnostico();
            os.AguardarAprovacao();
            os.Aprovar();
            os.ConcluirServico(servico.Id);

            os.Entregar();

            os.Status.Should().Be(EStatusOrdemServico.Entregue);
            os.EntregueEm.Should().NotBeNull();
        }

        [Fact(DisplayName = "Calculate Total Value Correctly")]
        public void ValorTotal_WithMultipleItems_ShouldSumCorrectly()
        {
            var os = new OrdemServico(_clienteIdValido, _veiculoIdValido, null);
            os.AdicionarServico(new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), 100.50m));
            os.AdicionarServico(new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), 50.00m));
            os.AdicionarInsumo(new OrdemServicoInsumo(Guid.NewGuid(), Guid.NewGuid(), "descricao", 25.00m, 2, new())); // 2 * 25 = 50

            os.ValorTotal.Should().Be(200.50m);
        }
    }
}
