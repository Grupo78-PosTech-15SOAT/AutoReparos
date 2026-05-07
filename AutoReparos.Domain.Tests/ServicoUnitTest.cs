using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.exceptions;
using FluentAssertions;

namespace AutoReparos.Domain.Tests
{
    public class ServicoUnitTest
    {
        [Fact(DisplayName = "Create Valid Service With Value")]
        public void CreateServico_WithValidData_ShouldSuccess()
        {
            var nome = "Alinhamento e Balanceamento";
            var valor = 150.00m;

            var servico = new Servico(nome, "Serviço completo de suspensão", valor);

            servico.Nome.Should().Be(nome);
            servico.ValorTabelado.Should().Be(valor);
            servico.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact(DisplayName = "Create Valid Service Without Value")]
        public void CreateServico_WithoutValue_ShouldSuccess()
        {
            var servico = new Servico("Diagnóstico", null, null);

            servico.ValorTabelado.Should().BeNull();
            servico.Nome.Should().Be("Diagnóstico");
        }

        [Fact(DisplayName = "Create Service With Empty Name")]
        public void CreateServico_WithEmptyName_ShouldThrowException()
        {
            Action action = () => new Servico("", "Desc", 50.00m);
            action.Should().Throw<InvalidServicoException>().WithMessage("Nome é obrigatório.");
        }

        [Fact(DisplayName = "Create Service With Name Too Long")]
        public void CreateServico_WithNameTooLong_ShouldThrowException()
        {
            var longName = new string('S', 101);
            Action action = () => new Servico(longName, "Desc", 50.00m);
            action.Should().Throw<InvalidServicoException>().WithMessage("Nome deve ter no máximo 100 caracteres.");
        }

        [Theory(DisplayName = "Create Service With Invalid Value")]
        [InlineData(0)]
        [InlineData(-10.50)]
        public void CreateServico_WithInvalidValue_ShouldThrowException(decimal valorInvalido)
        {
            Action action = () => new Servico("Troca de Óleo", null, valorInvalido);
            action.Should().Throw<InvalidServicoException>().WithMessage("Valor tabelado deve ser maior que zero.");
        }

        [Fact(DisplayName = "Update Service Successfully")]
        public void Atualizar_WithValidData_ShouldUpdatePropertiesAndSetDate()
        {
            var servico = new Servico("Nome Antigo", "Desc Antiga", 100.00m);

            servico.Atualizar("Nome Novo", "Desc Nova", 120.00m);

            servico.Nome.Should().Be("Nome Novo");
            servico.Descricao.Should().Be("Desc Nova");
            servico.ValorTabelado.Should().Be(120.00m);
            servico.AtualizadoEm.Should().NotBeNull();
            servico.AtualizadoEm.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact(DisplayName = "Update Service With Invalid Data")]
        public void Atualizar_WithInvalidData_ShouldThrowException()
        {
            var servico = new Servico("Limpeza", null, 80.00m);

            Action action = () => servico.Atualizar("", null, 0);
            action.Should().Throw<InvalidServicoException>();
        }
    }
}
