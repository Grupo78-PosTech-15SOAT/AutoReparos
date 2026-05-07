using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Exceptions;
using FluentAssertions;

namespace AutoReparos.Domain.Tests
{
    public class InsumoUnitTest
    {
        [Fact(DisplayName = "Create Valid Part")]
        public void CreateInsumo_WithValidData_ShouldSuccess()
        {
            var nome = "Pastilha de Freio";
            var valor = 85.50m;
            var estoque = 10;

            var insumo = new Insumo(nome, "Descrição detalhada", valor, estoque);

            insumo.Nome.Should().Be(nome);
            insumo.Valor.Should().Be(valor);
            insumo.QuantidadeEstoque.Should().Be(estoque);
            insumo.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact(DisplayName = "Create Part With Empty Name")]
        public void CreateInsumo_WithEmptyName_ShouldThrowException()
        {
            Action action = () => new Insumo("", "Desc", 10.00m, 5);
            action.Should().Throw<InvalidInsumoException>().WithMessage("Nome é obrigatório.");
        }

        [Fact(DisplayName = "Create Part With Name Too Long")]
        public void CreateInsumo_WithNameTooLong_ShouldThrowException()
        {
            var longName = new string('A', 101);
            Action action = () => new Insumo(longName, "Desc", 10.00m, 5);
            action.Should().Throw<InvalidInsumoException>().WithMessage("Nome deve ter no máximo 100 caracteres.");
        }

        [Theory(DisplayName = "Create Part With Invalid Value")]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreateInsumo_WithInvalidValue_ShouldThrowException(decimal valorInvalido)
        {
            Action action = () => new Insumo("Insumo", "Desc", valorInvalido, 5);
            action.Should().Throw<InvalidInsumoException>().WithMessage("Valor deve ser maior que zero.");
        }

        [Fact(DisplayName = "Update Part Successfully")]
        public void Atualizar_WithValidData_ShouldUpdatePropertiesAndSetDate()
        {
            var insumo = new Insumo("Nome Antigo", "Desc Antiga", 50.00m, 10);

            insumo.Atualizar("Nome Novo", "Desc Nova", 60.00m);

            insumo.Nome.Should().Be("Nome Novo");
            insumo.Valor.Should().Be(60.00m);
            insumo.AtualizadoEm.Should().NotBeNull();
        }

        [Fact(DisplayName = "Add Stock Successfully")]
        public void AdicionarEstoque_WithValidQuantity_ShouldIncrementValue()
        {
            var insumo = new Insumo("Insumo", null, 10.00m, 10);

            insumo.AdicionarEstoque(5);

            insumo.QuantidadeEstoque.Should().Be(15);
            insumo.AtualizadoEm.Should().NotBeNull();
        }

        [Theory(DisplayName = "Add Stock With Invalid Quantity")]
        [InlineData(0)]
        [InlineData(-5)]
        public void AdicionarEstoque_WithInvalidQuantity_ShouldThrowException(int qtdInvalida)
        {
            var insumo = new Insumo("Insumo", null, 10.00m, 10);

            Action action = () => insumo.AdicionarEstoque(qtdInvalida);

            action.Should().Throw<InvalidInsumoException>().WithMessage("Quantidade deve ser maior que zero.");
        }

        [Fact(DisplayName = "Remove Stock Successfully")]
        public void RemoverEstoque_WithValidQuantity_ShouldDecrementValue()
        {
            var insumo = new Insumo("Insumo", null, 10.00m, 10);

            insumo.RemoverEstoque(4);

            insumo.QuantidadeEstoque.Should().Be(6);
        }

        [Fact(DisplayName = "Remove More Than Available Stock")]
        public void RemoverEstoque_WhenQuantityIsGreater_ShouldThrowException()
        {
            var insumo = new Insumo("Insumo", null, 10.00m, 5);

            Action action = () => insumo.RemoverEstoque(6);

            action.Should().Throw<InvalidInsumoException>().WithMessage("Quantidade insuficiente em estoque.");
        }

        [Fact(DisplayName = "Remove Stock With Invalid Quantity")]
        public void RemoverEstoque_WithZeroOrNegative_ShouldThrowException()
        {
            var insumo = new Insumo("Insumo", null, 10.00m, 10);

            Action action = () => insumo.RemoverEstoque(0);

            action.Should().Throw<InvalidInsumoException>().WithMessage("Quantidade deve ser maior que zero.");
        }
    }
}
