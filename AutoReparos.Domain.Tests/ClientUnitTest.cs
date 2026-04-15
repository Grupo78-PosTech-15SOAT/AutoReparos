using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.Clientes.ValueObjects.Exceptions;
using FluentAssertions;

namespace AutoReparos.Domain.Tests
{
    public class ClientUnitTest
    {
        [Fact(DisplayName = "Create Client With Valid CPF")]
        public void CreateClient_WithValidCpf()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("52998224725"), "912345678", Email.Create("client@example.com"));
            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Valid CNPJ")]
        public void CreateClient_WithValidCnpj()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("59178174000191"), "912345678", Email.Create("client@example.com"));
            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Formatted CPF")]
        public void CreateClient_WithFormattedCpf()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("529.982.247-25"), "912345678", Email.Create("client@example.com"));
            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Formatted CNPJ")]
        public void CreateClient_WithFormattedCnpj()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("59.178.174/0001-91"), "912345678", Email.Create("client@example.com"));
            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Name At Max Length")]
        public void CreateClient_WithNameAtMaxLength()
        {
            var name = new string('A', 100);
            Action action = () => new Cliente(name, new Documento("52998224725"), "912345678", Email.Create("client@example.com"));
            action.Should().NotThrow();
        }

        [Fact(DisplayName = "Create Client With Name Empty")]
        public void CreateClient_WithNameEmpty()
        {
            Action action = () => new Cliente("", new Documento("99806446054"), "912345678", Email.Create("client@example.com"));
            action.Should().Throw<InvalidClienteException>().WithMessage("Nome é obrigatório");
        }

        [Fact(DisplayName = "Create Client With Name Whitespace")]
        public void CreateClient_WithNameWhitespace()
        {
            Action action = () => new Cliente("   ", new Documento("99806446054"), "912345678", Email.Create("client@example.com"));
            action.Should().Throw<InvalidClienteException>().WithMessage("Nome é obrigatório");
        }

        [Fact(DisplayName = "Create Client With Name Too Long")]
        public void CreateClient_WithNameTooLong()
        {
            var longName = new string('A', 101);
            Action action = () => new Cliente(longName, new Documento("59178174000191"), "912345678", Email.Create("client@example.com"));
            action.Should().Throw<InvalidClienteException>().WithMessage("Nome muito longo, o nome deve ter no máximo 100 caracteres.");
        }

        [Fact(DisplayName = "Create Client With Document Empty")]
        public void CreateClient_WithDocumentEmpty()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento(""), "912345678", Email.Create("client@example.com"));
            action.Should().Throw<InvalidDocumentoException>().WithMessage("O CPF ou CNPJ é obrigatório.");
        }

        [Fact(DisplayName = "Create Client With Invalid Document")]
        public void CreateClient_WithInvalidDocument()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("123"), "912345678", Email.Create("client@example.com"));
            action.Should().Throw<InvalidDocumentoException>().WithMessage("CPF ou CNPJ inválido.");
        }

        [Fact(DisplayName = "Create Client With Phone Empty")]
        public void CreateClient_WithPhoneEmpty()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("62040533000154"), "", Email.Create("client@example.com"));
            action.Should().Throw<InvalidClienteException>().WithMessage("Telefone é obrigatório");
        }

        [Fact(DisplayName = "Create Client With Phone Whitespace")]
        public void CreateClient_WithPhoneWhitespace()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("62040533000154"), "   ", Email.Create("client@example.com"));
            action.Should().Throw<InvalidClienteException>().WithMessage("Telefone é obrigatório");
        }

        [Fact(DisplayName = "Create Client With Phone Less Than 9 Digits")]
        public void CreateClient_WithPhoneLessThan9Digits()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("52998224725"), "91234567", Email.Create("client@example.com"));
            action.Should().Throw<InvalidClienteException>().WithMessage("*9 dígitos*");
        }

        [Fact(DisplayName = "Create Client With Phone More Than 9 Digits")]
        public void CreateClient_WithPhoneMoreThan9Digits()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("52998224725"), "9123456789", Email.Create("client@example.com"));
            action.Should().Throw<InvalidClienteException>().WithMessage("*9 dígitos*");
        }

        [Fact(DisplayName = "Create Client With Phone Not Starting With 9")]
        public void CreateClient_WithPhoneNotStartingWith9()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("52998224725"), "812345678", Email.Create("client@example.com"));
            action.Should().Throw<InvalidClienteException>().WithMessage("*começar com 9*");
        }

        [Fact(DisplayName = "Create Client With Email Empty")]
        public void CreateClient_WithEmailEmpty()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("25722271000181"), "912345678", Email.Create(""));
            action.Should().Throw<InvalidEmailException>().WithMessage("E-mail é obrigatório.");
        }

        [Fact(DisplayName = "Create Client With Invalid Email")]
        public void CreateClient_WithInvalidEmail()
        {
            Action action = () => new Cliente("Cliente Teste", new Documento("52998224725"), "912345678", Email.Create("clientexample.com"));
            action.Should().Throw<InvalidEmailException>().WithMessage("Endereço de E-mail inválido.");
        }
    }
}