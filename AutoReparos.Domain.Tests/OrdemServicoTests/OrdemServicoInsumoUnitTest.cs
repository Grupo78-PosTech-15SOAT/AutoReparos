using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Exceptions;
using FluentAssertions;

namespace AutoReparos.Domain.Tests.OrdemServicoTests
{
    public class OrdemServicoInsumoUnitTest
    {
        private readonly Guid _osId = Guid.NewGuid();
        private readonly Guid _insumoId = Guid.NewGuid();

        [Fact(DisplayName = "Create Valid External Part")]
        public void CreateInsumo_WithValidExternalData_ShouldSuccess()
        {
            var action = () => new OrdemServicoInsumo(
                _osId,
                null,
                "Filtro Amortecedor",
                150.00m,
                2,
                EOrigemInsumo.CompraEspecifica);

            var insumo = action.Should().NotThrow().Subject;
            insumo.ValorTotal.Should().Be(300.00m);
        }

        [Fact(DisplayName = "Create Valid Stock Part")]
        public void CreateInsumo_WithValidStockData_ShouldSuccess()
        {
            var action = () => new OrdemServicoInsumo(
                _osId,
                _insumoId,
                "Pastilha de Freio",
                80.00m,
                1,
                EOrigemInsumo.Estoque);

            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Stock Part Without Reference")]
        public void CreateInsumo_StockOriginWithoutId_ShouldThrowException()
        {
            Action action = () => new OrdemServicoInsumo(
                _osId,
                null,
                "Óleo 5W30",
                45.00m,
                4,
                EOrigemInsumo.Estoque);

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Insumo do estoque deve ter referência ao cadastro.");
        }

        [Fact(DisplayName = "Create Part With Empty Description")]
        public void CreateInsumo_WithEmptyDescription_ShouldThrowException()
        {
            Action action = () => new OrdemServicoInsumo(
                _osId,
                null,
                "",
                10.00m,
                1,
                EOrigemInsumo.CompraEspecifica);

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Descrição é obrigatória.");
        }

        [Theory(DisplayName = "Create Part With Invalid Price")]
        [InlineData(0)]
        [InlineData(-10)]
        public void CreateInsumo_WithInvalidPrice_ShouldThrowException(decimal valorInvalido)
        {
            Action action = () => new OrdemServicoInsumo(
                _osId,
                null,
                "Insumo Teste",
                valorInvalido,
                1,
                EOrigemInsumo.CompraEspecifica);

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Valor unitário deve ser maior que zero.");
        }

        [Theory(DisplayName = "Create Part With Invalid Quantity")]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreateInsumo_WithInvalidQuantity_ShouldThrowException(int qtdInvalida)
        {
            Action action = () => new OrdemServicoInsumo(
                _osId,
                null,
                "Insumo Teste",
                100.00m,
                qtdInvalida,
                EOrigemInsumo.CompraEspecifica);

            action.Should().Throw<InvalidOrdemServicoException>()
                .WithMessage("Quantidade deve ser maior que zero.");
        }

        [Fact(DisplayName = "Calculate Total Value Correctly")]
        public void ValorTotal_ShouldCalculateMultiplyQuantityByUnitPrice()
        {
            var valorUnitario = 12.50m;
            var quantidade = 4;
            var esperado = 50.00m;

            var insumo = new OrdemServicoInsumo(
                _osId,
                null,
                "Parafuso",
                valorUnitario,
                quantidade,
                EOrigemInsumo.CompraEspecifica);

            insumo.ValorTotal.Should().Be(esperado);
        }
    }
}
