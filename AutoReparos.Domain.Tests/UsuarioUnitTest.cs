using AutoReparos.Domain.Clientes.ValueObjects;
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
            var email = Email.Create("test@example.com");
            var tipo = ETipoUsuario.Mecanico;
            var usuario = new Usuario(nome, email, tipo);

            usuario.NomeCompleto.Should().Be(nome);
            usuario.Email.Should().Be(email.Address);
            usuario.UserName.Should().Be(email.Address);
            usuario.Tipo.Should().Be(tipo);
            usuario.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact(DisplayName = "Create Usuario With Empty Name")]
        public void CreateUsuario_WithEmptyName_ShouldThrowException()
        {
            var email = Email.Create("test@example.com");
            Action action = () => new Usuario("", email, ETipoUsuario.Atendente);

            action.Should().Throw<ArgumentException>().WithMessage("Nome completo é obrigatório*");
        }

        [Fact(DisplayName = "Create Usuario With Name Too Long")]
        public void CreateUsuario_WithNameTooLong_ShouldThrowException()
        {
            var longName = new string('A', 151);
            var email = Email.Create("test@example.com");
            Action action = () => new Usuario(longName, email, ETipoUsuario.Administrador);

            action.Should().Throw<ArgumentException>().WithMessage("Nome completo muito longo*");
        }

        [Fact(DisplayName = "Update Usuario Successfully")]
        public void Atualizar_WithValidData_ShouldUpdateProperties()
        {
            var usuario = new Usuario("Nome Antigo", Email.Create("old@example.com"), ETipoUsuario.Atendente);
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
            var usuario = new Usuario("Nome", Email.Create("test@example.com"), ETipoUsuario.Mecanico);
            Action action = () => usuario.Atualizar("", ETipoUsuario.Administrador);

            action.Should().Throw<ArgumentException>().WithMessage("Nome completo é obrigatório*");
        }
    }
}
