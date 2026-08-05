using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using FluentAssertions;

namespace AutoReparos.Domain.Tests
{
    public class UsuarioUnitTest
    {
        [Fact(DisplayName = "Create Valid Usuario")]
        public void CreateUsuario_WithValidData_ShouldSuccess()
        {
            var nome = "Usuario Teste";
            var email = "test@example.com";
            var tipo = ETipoUsuario.Mecanico;
            var usuario = new Usuario(nome, email, tipo);

            usuario.NomeCompleto.Should().Be(nome);
            usuario.Email.Endereco.Should().Be(email);
            usuario.Tipo.Should().Be(tipo);
            usuario.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact(DisplayName = "Create Usuario With Empty Name")]
        public void CreateUsuario_WithEmptyName_ShouldThrowException()
        {
            var email = "test@example.com";
            Action action = () => new Usuario("", email, ETipoUsuario.Atendente);

            action.Should().Throw<ArgumentException>().WithMessage("Nome completo é obrigatório*");
        }

        [Fact(DisplayName = "Create Usuario With Name Too Long")]
        public void CreateUsuario_WithNameTooLong_ShouldThrowException()
        {
            var longName = new string('A', 151);
            var email = "test@example.com";
            Action action = () => new Usuario(longName, email, ETipoUsuario.Administrador);

            action.Should().Throw<ArgumentException>().WithMessage("Nome completo muito longo*");
        }

        [Fact(DisplayName = "Update Usuario Successfully")]
        public void Atualizar_WithValidData_ShouldUpdateProperties()
        {
            var usuario = new Usuario("Nome Antigo", "old@example.com", ETipoUsuario.Atendente);
            var novoNome = "Nome Novo";
            var novoTipo = ETipoUsuario.Administrador;
            usuario.Atualizar(novoNome, novoTipo);

            usuario.NomeCompleto.Should().Be(novoNome);
            usuario.Tipo.Should().Be(novoTipo);
            usuario.AtualizadoEm.Should().NotBeNull();
            usuario.AtualizadoEm.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact(DisplayName = "Update Usuario With Empty Name")]
        public void Atualizar_WithEmptyName_ShouldThrowException()
        {
            var usuario = new Usuario("Nome", "test@example.com", ETipoUsuario.Mecanico);
            Action action = () => usuario.Atualizar("", ETipoUsuario.Administrador);

            action.Should().Throw<ArgumentException>().WithMessage("Nome completo é obrigatório*");
        }

        [Fact(DisplayName = "Load Existing Usuario Successfully")]
        public void Load_WithValidData_ShouldRestoreState()
        {
            var id = Guid.NewGuid();
            var nome = "Existente";
            var email = "existente@test.com";
            var tipo = ETipoUsuario.Administrador;
            var criadoEm = DateTime.UtcNow.AddDays(-1);
            var atualizadoEm = DateTime.UtcNow;

            var usuario = Usuario.Load(id, nome, email, tipo, criadoEm, atualizadoEm);

            usuario.Id.Should().Be(id);
            usuario.NomeCompleto.Should().Be(nome);
            usuario.Email.Endereco.Should().Be(email);
            usuario.Tipo.Should().Be(tipo);
            usuario.CriadoEm.Should().Be(criadoEm);
            usuario.AtualizadoEm.Should().Be(atualizadoEm);
        }

        [Theory(DisplayName = "Usuario Creation With Empty Or Null Name Should Throw ArgumentException")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void UsuarioCreation_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
        {
            // Arrange & Act
            Action act = () => new Usuario(invalidName!, "test@example.com", ETipoUsuario.Mecanico);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory(DisplayName = "Usuario Creation With Empty Or Null Email Should Throw DomainException")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void UsuarioCreation_WithInvalidEmail_ShouldThrowDomainException(string? invalidEmail)
        {
            // Arrange & Act
            Action act = () => new Usuario("John Doe", invalidEmail!, ETipoUsuario.Mecanico);

            // Assert
            act.Should().Throw<DomainException>();
        }
    }
}

