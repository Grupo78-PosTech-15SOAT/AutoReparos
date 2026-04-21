using AutoReparos.Domain.Pecas.Entities;
using AutoReparos.Domain.Pecas.Exceptions;
using FluentAssertions;

namespace AutoReparos.Domain.Tests
{
    public class PecaUnitTest
    {
        [Fact(DisplayName = "Create Valid Part")]
        public void CreatePeca_WithValidData_ShouldSuccess()
        {
            var nome = "Pastilha de Freio";
            var valor = 85.50m;
            var estoque = 10;

            var peca = new Peca(nome, "Descrição detalhada", valor, estoque);

            peca.Nome.Should().Be(nome);
            peca.Valor.Should().Be(valor);
            peca.QuantidadeEstoque.Should().Be(estoque);
            peca.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact(DisplayName = "Create Part With Empty Name")]
        public void CreatePeca_WithEmptyName_ShouldThrowException()
        {
            Action action = () => new Peca("", "Desc", 10.00m, 5);
            action.Should().Throw<InvalidPecaException>().WithMessage("Nome é obrigatório.");
        }

        [Fact(DisplayName = "Create Part With Name Too Long")]
        public void CreatePeca_WithNameTooLong_ShouldThrowException()
        {
            var longName = new string('A', 101);
            Action action = () => new Peca(longName, "Desc", 10.00m, 5);
            action.Should().Throw<InvalidPecaException>().WithMessage("Nome deve ter no máximo 100 caracteres.");
        }

        [Theory(DisplayName = "Create Part With Invalid Value")]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreatePeca_WithInvalidValue_ShouldThrowException(decimal valorInvalido)
        {
            Action action = () => new Peca("Peca", "Desc", valorInvalido, 5);
            action.Should().Throw<InvalidPecaException>().WithMessage("Valor deve ser maior que zero.");
        }

        [Fact(DisplayName = "Update Part Successfully")]
        public void Atualizar_WithValidData_ShouldUpdatePropertiesAndSetDate()
        {
            var peca = new Peca("Nome Antigo", "Desc Antiga", 50.00m, 10);

            peca.Atualizar("Nome Novo", "Desc Nova", 60.00m);

            peca.Nome.Should().Be("Nome Novo");
            peca.Valor.Should().Be(60.00m);
            peca.AtualizadoEm.Should().NotBeNull();
        }

        [Fact(DisplayName = "Add Stock Successfully")]
        public void AdicionarEstoque_WithValidQuantity_ShouldIncrementValue()
        {
            var peca = new Peca("Peca", null, 10.00m, 10);

            peca.AdicionarEstoque(5);

            peca.QuantidadeEstoque.Should().Be(15);
            peca.AtualizadoEm.Should().NotBeNull();
        }

        [Theory(DisplayName = "Add Stock With Invalid Quantity")]
        [InlineData(0)]
        [InlineData(-5)]
        public void AdicionarEstoque_WithInvalidQuantity_ShouldThrowException(int qtdInvalida)
        {
            var peca = new Peca("Peca", null, 10.00m, 10);

            Action action = () => peca.AdicionarEstoque(qtdInvalida);

            action.Should().Throw<InvalidPecaException>().WithMessage("Quantidade deve ser maior que zero.");
        }

        [Fact(DisplayName = "Remove Stock Successfully")]
        public void RemoverEstoque_WithValidQuantity_ShouldDecrementValue()
        {
            var peca = new Peca("Peca", null, 10.00m, 10);

            peca.RemoverEstoque(4);

            peca.QuantidadeEstoque.Should().Be(6);
        }

        [Fact(DisplayName = "Remove More Than Available Stock")]
        public void RemoverEstoque_WhenQuantityIsGreater_ShouldThrowException()
        {
            var peca = new Peca("Peca", null, 10.00m, 5);

            Action action = () => peca.RemoverEstoque(6);

            action.Should().Throw<InvalidPecaException>().WithMessage("Quantidade insuficiente em estoque.");
        }

        [Fact(DisplayName = "Remove Stock With Invalid Quantity")]
        public void RemoverEstoque_WithZeroOrNegative_ShouldThrowException()
        {
            var peca = new Peca("Peca", null, 10.00m, 10);

            Action action = () => peca.RemoverEstoque(0);

            action.Should().Throw<InvalidPecaException>().WithMessage("Quantidade deve ser maior que zero.");
        }
    }
}
