using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Exceptions;
using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.Shared.ValueObjects;
using FluentAssertions;

namespace AutoReparos.Domain.Tests
{
    public class ClienteUnitTest
    {
        #region CPF e CNPJ

        [Fact(DisplayName = "Create Client With Valid CPF")]
        public void CreateClient_WithValidCpf()
        {
            Action action = () => new Cliente("Cliente Teste", "66265464060", "11912345678", "client@example.com");
            action.Should().NotThrow<InvalidDocumentoException>();
        }

        [Fact(DisplayName = "Create Client With Valid CNPJ")]
        public void CreateClient_WithValidCnpj()
        {
            Action action = () => new Cliente("Cliente Teste", "59178174000191", "11912345678", "client@example.com");
            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Formatted CPF")]
        public void CreateClient_WithFormattedCpf()
        {
            Action action = () => new Cliente("Cliente Teste", "496.330.750-25", "11912345678", "client@example.com");
            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Formatted CNPJ")]
        public void CreateClient_WithFormattedCnpj()
        {
            Action action = () => new Cliente("Cliente Teste", "21.303.244/0001-13", "11912345678", "client@example.com");
            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Document Empty")]
        public void CreateClient_WithDocumentEmpty()
        {
            Action action = () => new Cliente("Cliente Teste", "", "912345678", "client@example.com");
            action.Should().Throw<InvalidDocumentoException>().WithMessage("O CPF ou CNPJ é obrigatório.");
        }

        [Fact(DisplayName = "Create Client With Invalid Document")]
        public void CreateClient_WithInvalidDocument()
        {
            Action action = () => new Cliente("Cliente Teste", "123", "11912345678", "client@example.com");
            action.Should().Throw<InvalidDocumentoException>().WithMessage("CPF ou CNPJ inválido.");
        }

        #endregion

        #region Nome

        [Fact(DisplayName = "Create Client With Name At Max Length")]
        public void CreateClient_WithNameAtMaxLength()
        {
            var name = new string('A', 100);
            Action action = () => new Cliente(name, "52998224725", "11912345678", "client@example.com");
            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Name Empty")]
        public void CreateClient_WithNameEmpty()
        {
            Action action = () => new Cliente("", "99806446054", "11912345678", "client@example.com");
            action.Should().Throw<InvalidClienteException>().WithMessage("Nome é obrigatório");
        }

        [Fact(DisplayName = "Create Client With Name Whitespace")]
        public void CreateClient_WithNameWhitespace()
        {
            Action action = () => new Cliente("   ", "99806446054", "11912345678", "client@example.com");
            action.Should().Throw<InvalidClienteException>().WithMessage("Nome é obrigatório");
        }

        [Fact(DisplayName = "Create Client With Name Too Long")]
        public void CreateClient_WithNameTooLong()
        {
            var longName = new string('A', 101);
            Action action = () => new Cliente(longName, "59178174000191", "11912345678", "client@example.com");
            action.Should().Throw<InvalidClienteException>().WithMessage("Nome muito longo, o nome deve ter no máximo 100 caracteres.");
        }

        #endregion

        #region Telefone

        [Fact(DisplayName = "Create Client With Phone Empty")]
        public void CreateClient_WithPhoneEmpty()
        {
            Action action = () => new Cliente("Cliente Teste", "62040533000154", "", "client@example.com");

            action.Should()
                .Throw<InvalidTelefoneException>()
                .WithMessage("O telefone é obrigatório.");
        }

        [Fact(DisplayName = "Create Client With Phone Whitespace")]
        public void CreateClient_WithPhoneWhitespace()
        {
            Action action = () => new Cliente("Cliente Teste", "62040533000154", "     ", "client@example.com");

            action.Should()
                .Throw<InvalidTelefoneException>()
                .WithMessage("O telefone é obrigatório.");
        }

        [Fact(DisplayName = "Create Client With Valid Fixed Phone")]
        public void CreateClient_WithValidFixedPhone()
        {
            Action action = () => new Cliente("Cliente Teste", "62040533000154", "1133334444", "client@example.com");

            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Valid Mobile Phone")]
        public void CreateClient_WithValidMobilePhone()
        {
            Action action = () => new Cliente("Cliente Teste", "62040533000154", "11912345678", "client@example.com");

            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Formatted Mobile Phone")]
        public void CreateClient_WithFormattedMobilePhone()
        {
            Action action = () => new Cliente("Cliente Teste", "62040533000154", "(11) 91234-5678", "client@example.com");

            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Invalid Phone Less Than 10 Digits")]
        public void CreateClient_WithPhoneLessThan10Digits()
        {
            Action action = () => new Cliente("Cliente Teste", "62040533000154", "912345678", "client@example.com");

            action.Should()
                .Throw<InvalidTelefoneException>()
                .WithMessage("Telefone inválido.");
        }

        [Fact(DisplayName = "Create Client With Invalid Phone More Than 11 Digits")]
        public void CreateClient_WithPhoneMoreThan11Digits()
        {
            Action action = () => new Cliente("Cliente Teste", "62040533000154", "119123456789", "client@example.com");

            action.Should()
                .Throw<InvalidTelefoneException>()
                .WithMessage("Telefone inválido.");
        }

        [Fact(DisplayName = "Create Client With Phone Starting With Zero")]
        public void CreateClient_WithPhoneStartingWithZero()
        {
            Action action = () => new Cliente("Cliente Teste", "62040533000154", "01912345678", "client@example.com");

            action.Should()
                .Throw<InvalidTelefoneException>()
                .WithMessage("Telefone inválido.");
        }

        [Fact(DisplayName = "Create Client With Phone Containing Letters")]
        public void CreateClient_WithPhoneContainingLetters()
        {
            Action action = () => new Cliente("Cliente Teste", "62040533000154", "11ABC345678", "client@example.com");

            action.Should()
                .Throw<InvalidTelefoneException>()
                .WithMessage("Telefone inválido.");
        }

        #endregion


        #region E-mail

        [Fact(DisplayName = "Create Client With Email Empty")]
        public void CreateClient_WithEmailEmpty()
        {
            Action action = () => new Cliente("Cliente Teste", "25722271000181", "11912345678", "");
            action.Should().Throw<InvalidEmailException>().WithMessage("E-mail é obrigatório.");
        }

        [Fact(DisplayName = "Create Client With Invalid Email")]
        public void CreateClient_WithInvalidEmail()
        {
            Action action = () => new Cliente("Cliente Teste", "52998224725", "11912345678", "clientexample.com");
            action.Should().Throw<InvalidEmailException>().WithMessage("Endereço de E-mail inválido.");
        }

        #endregion
    }
}