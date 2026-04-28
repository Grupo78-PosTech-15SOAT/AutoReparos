using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Exceptions;
using AutoReparos.Domain.Veiculos.ValueObjects;
using FluentAssertions;

namespace AutoReparos.Domain.Tests
{
    public class VeiculoUnitTest
    {
        [Fact(DisplayName = "Create Vehicle With Cliente Empty")]
        public void CreateVehicle_WithClienteEmpty()
        {
            Action action = () => new Veiculo(Guid.Empty, "Chevrolet", "", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            action.Should().Throw<InvalidVeiculoException>().WithMessage("Cliente é obrigatório");
        }

        [Fact(DisplayName = "Create Vehicle With Modelo Empty")]
        public void CreateVehicle_WithModeloEmpty()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "Chevrolet", "", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            action.Should().Throw<InvalidVeiculoException>().WithMessage("Modelo é obrigatório");
        }

        [Fact(DisplayName = "Create Vehicle With Marca Empty")]
        public void CreateVehicle_WithMarcaEmpty()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            action.Should().Throw<InvalidVeiculoException>().WithMessage("Marca é obrigatória");
        }

        [Fact(DisplayName = "Create Vehicle With AnoFabricacao Less Than 1900")]
        public void CreateVehicle_WithAnoFabricacaoLessThan1900()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 1800, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            action.Should().Throw<InvalidVeiculoException>().WithMessage("Ano de fabricação inválido");
        }

        [Fact(DisplayName = "Create Vehicle With AnoFabricacao Greater Than Allowed Year")]
        public void CreateVehicle_WithAnoFabricacaoGreaterThanAllowedYear()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2028, 2028, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            action.Should().Throw<InvalidVeiculoException>().WithMessage("Ano de fabricação inválido");
        }

        [Fact(DisplayName = "Create Vehicle With AnoModelo Less Than AnoFabricacao")]
        public void CreateVehicle_WithAnoModeloLessThanAnoFabricacao()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2019, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            action.Should().Throw<InvalidVeiculoException>().WithMessage("Ano modelo não pode ser menor que fabricação");
        }

        [Fact(DisplayName = "Create Vehicle With Valid Parameters")]
        public void CreateVehicle_WithValidParameters()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Vehicle With Placa Empty")]
        public void CreateVehicle_WithPlacaEmpty()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa(""), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            action.Should().Throw<InvalidPlacaException>().WithMessage("Placa é obrigatória");
        }

        [Fact(DisplayName = "Create Vehicle With Chassi Empty")]
        public void CreateVehicle_WithChassiEmpty()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi(""), new Renavam("00123456789"));
            action.Should().Throw<InvalidChassiException>().WithMessage("Chassi é obrigatório");
        }

        [Fact(DisplayName = "Create Vehicle With Renavam Empty")]
        public void CreateVehicle_WithRenavamEmpty()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam(""));
            action.Should().Throw<InvalidRenavamException>().WithMessage("Renavam é obrigatório");
        }

        [Fact(DisplayName = "Create Vehicle With Placa Invalid")]
        public void CreateVehicle_WithPlacaInvalid()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D2"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            action.Should().Throw<InvalidPlacaException>().WithMessage("Placa inválida");
        }

        [Fact(DisplayName = "Create Vehicle With Chassi Less Than 17 Characters")]
        public void CreateVehicle_WithChassiLessThan17Characters()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T50021"), new Renavam("00123456789"));
            action.Should().Throw<InvalidChassiException>().WithMessage("Chassi deve ter 17 caracteres");
        }

        [Fact(DisplayName = "Create Vehicle With Renavam Invalid")]
        public void CreateVehicle_WithRenavamInvalid()
        {
            Action action = () => new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("001234567"));
            action.Should().Throw<InvalidRenavamException>().WithMessage("Renavam inválido");
        }

        [Fact(DisplayName = "Update Vehicle With Valid Parameters")]
        public void UpdateVehicle_WithValidParameters()
        {
            var veiculo = new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            Action action = () => veiculo.Atualizar("Chevrolet", "Onix", 2020, 2021);
            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Update Vehicle With Marca Empty")]
        public void UpdateVehicle_WithMarcaEmpty()
        {
            var veiculo = new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            Action action = () => veiculo.Atualizar("", "Onix", 2020, 2021);
            action.Should().Throw<InvalidVeiculoException>().WithMessage("Marca é obrigatória");
        }

        [Fact(DisplayName = "Update Vehicle With Modelo Empty")]
        public void UpdateVehicle_WithModeloEmpty()
        {
            var veiculo = new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            Action action = () => veiculo.Atualizar("Chevrolet", "", 2020, 2021);
            action.Should().Throw<InvalidVeiculoException>().WithMessage("Modelo é obrigatório");
        }

        [Fact(DisplayName = "Update Vehicle With AnoFabricacao less Than 1900")]
        public void UpdateVehicle_WithAnoFabricacaoLessThan1900()
        {
            var veiculo = new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            Action action = () => veiculo.Atualizar("Chevrolet", "Onix", 1899, 2021);
            action.Should().Throw<InvalidVeiculoException>().WithMessage("Ano de fabricação inválido");
        }

        [Fact(DisplayName = "Update Vehicle With AnoFabricacao Greater Than Allowed Year")]
        public void UpdateVehicle_WithAnoFabricacaoGreaterThanAllowedYear()
        {
            var veiculo = new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            Action action = () => veiculo.Atualizar("Chevrolet", "Onix", 2028, 2028);
            action.Should().Throw<InvalidVeiculoException>().WithMessage("Ano de fabricação inválido");
        }

        [Fact(DisplayName = "Update Vehicle With AnoModelo Less Than AnoFabricacao")]
        public void UpdateVehicle_WithAnoModeloLessThanAnoFabricacao()
        {
            var veiculo = new Veiculo(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("abc1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"));
            Action action = () => veiculo.Atualizar("Chevrolet", "Onix", 2020, 2019);
            action.Should().Throw<InvalidVeiculoException>().WithMessage("Ano modelo não pode ser menor que fabricação");
        }
    }
}