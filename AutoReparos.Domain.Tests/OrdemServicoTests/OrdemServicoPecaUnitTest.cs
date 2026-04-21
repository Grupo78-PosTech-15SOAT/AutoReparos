using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Exceptions;
using FluentAssertions;

namespace AutoReparos.Domain.Tests.OrdemServicoTests
{
    public class OrdemServicoPecaUnitTest
    {
        private readonly Guid _osId = Guid.NewGuid();
        private readonly Guid _pecaId = Guid.NewGuid();

        [Fact(DisplayName = "Create Valid External Part")]
        public void CreatePeca_WithValidExternalData_ShouldSuccess()
        {
            var action = () => new OrdemServicoPeca(
                _osId,
                null,
                "Filtro Amortecedor",
                150.00m,
                2,
                EOrigemPeca.CompraEspecifica);

            var peca = action.Should().NotThrow().Subject;
            peca.ValorTotal.Should().Be(300.00m);
        }

        [Fact(DisplayName = "Create Valid Stock Part")]
        public void CreatePeca_WithValidStockData_ShouldSuccess()
        {
            var action = () => new OrdemServicoPeca(
                _osId,
                _pecaId,
                "Pastilha de Freio",
                80.00m,
                1,
                EOrigemPeca.Estoque);

            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Stock Part Without Reference")]
        public void CreatePeca_StockOriginWithoutId_ShouldThrowException()
        {
            Action action = () => new OrdemServicoPeca(
                _osId,
                null,
                "Óleo 5W30",
                45.00m,
                4,
                EOrigemPeca.Estoque);

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Peça do estoque deve ter referência ao cadastro.");
        }

        [Fact(DisplayName = "Create Part With Empty Description")]
        public void CreatePeca_WithEmptyDescription_ShouldThrowException()
        {
            Action action = () => new OrdemServicoPeca(
                _osId,
                null,
                "",
                10.00m,
                1,
                EOrigemPeca.CompraEspecifica);

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Descrição é obrigatória.");
        }

        [Theory(DisplayName = "Create Part With Invalid Price")]
        [InlineData(0)]
        [InlineData(-10)]
        public void CreatePeca_WithInvalidPrice_ShouldThrowException(decimal valorInvalido)
        {
            Action action = () => new OrdemServicoPeca(
                _osId,
                null,
                "Peca Teste",
                valorInvalido,
                1,
                EOrigemPeca.CompraEspecifica);

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Valor unitário deve ser maior que zero.");
        }

        [Theory(DisplayName = "Create Part With Invalid Quantity")]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreatePeca_WithInvalidQuantity_ShouldThrowException(int qtdInvalida)
        {
            Action action = () => new OrdemServicoPeca(
                _osId,
                null,
                "Peca Teste",
                100.00m,
                qtdInvalida,
                EOrigemPeca.CompraEspecifica);

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Quantidade deve ser maior que zero.");
        }

        [Fact(DisplayName = "Calculate Total Value Correctly")]
        public void ValorTotal_ShouldCalculateMultiplyQuantityByUnitPrice()
        {
            var valorUnitario = 12.50m;
            var quantidade = 4;
            var esperado = 50.00m;

            var peca = new OrdemServicoPeca(
                _osId,
                null,
                "Parafuso",
                valorUnitario,
                quantidade,
                EOrigemPeca.CompraEspecifica);

            peca.ValorTotal.Should().Be(esperado);
        }
    }
}
