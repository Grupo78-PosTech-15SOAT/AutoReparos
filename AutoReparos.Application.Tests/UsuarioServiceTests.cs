using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.Services;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Usuarios.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace AutoReparos.Application.Tests
{
    public class UsuarioServiceTests
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly UsuarioService _usuarioService;

        public UsuarioServiceTests()
        {
            _userManager = IdentityMockHelper.MockUserManager<Usuario>();
            _usuarioService = new UsuarioService(_userManager);
        }

        [Fact(DisplayName = "Create User Successfully")]
        public async Task Create_WithValidData_ShouldReturnDto()
        {
            var dto = new UsuarioCreateDTO("Novo Usuario", "novo@test.com", "Pass123!", ETipoUsuario.Atendente);
            _userManager.CreateAsync(Arg.Any<Usuario>(), dto.Password).Returns(IdentityResult.Success);

            var result = await _usuarioService.Create(dto);

            result.Should().NotBeNull();
            result.NomeCompleto.Should().Be(dto.NomeCompleto);
            result.Email.Should().Be(dto.Email);
        }

        [Fact(DisplayName = "Create User With Duplicate Email Should Throw Exception")]
        public async Task Create_WithDuplicateEmail_ShouldThrowException()
        {
            var dto = new UsuarioCreateDTO("Novo Usuario", "duplicado@test.com", "Pass123!", ETipoUsuario.Atendente);
            var error = new IdentityError { Code = "DuplicateEmail", Description = $"O e-mail '{dto.Email}' já está sendo utilizado." };
            _userManager.CreateAsync(Arg.Any<Usuario>(), dto.Password).Returns(IdentityResult.Failed(error));

            Func<Task> action = async () => await _usuarioService.Create(dto);

            await action.Should().ThrowAsync<InvalidUsuarioException>()
                .WithMessage($"*O e-mail '{dto.Email}' já está sendo utilizado.*");
        }

        [Fact(DisplayName = "Get User By Id Successfully")]
        public async Task GetById_WhenUserExists_ShouldReturnDto()
        {
            var userId = Guid.NewGuid();
            var user = new Usuario("Test", Domain.Clientes.ValueObjects.Email.Create("test@test.com"), ETipoUsuario.Mecanico);
            _userManager.FindByIdAsync(userId.ToString()).Returns(user);

            var result = await _usuarioService.GetById(userId);

            result.Should().NotBeNull();
            result.NomeCompleto.Should().Be(user.NomeCompleto);
        }

        [Fact(DisplayName = "Get User By Id Not Found Should Throw Exception")]
        public async Task GetById_WhenUserDoesNotExist_ShouldThrowNotFound()
        {
            var userId = Guid.NewGuid();
            _userManager.FindByIdAsync(userId.ToString()).Returns((Usuario?)null);

            Func<Task> action = async () => await _usuarioService.GetById(userId);

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "Update User Successfully")]
        public async Task Update_WhenUserExists_ShouldUpdateAndCallUpdateAsync()
        {
            var userId = Guid.NewGuid();
            var user = new Usuario("Antigo", Domain.Clientes.ValueObjects.Email.Create("test@test.com"), ETipoUsuario.Atendente);
            var dto = new UsuarioUpdateDTO("Novo Nome", ETipoUsuario.Administrador);

            _userManager.FindByIdAsync(userId.ToString()).Returns(user);
            _userManager.UpdateAsync(user).Returns(IdentityResult.Success);

            await _usuarioService.Update(userId, dto);

            user.NomeCompleto.Should().Be(dto.NomeCompleto);
            user.Tipo.Should().Be(dto.Tipo);
            await _userManager.Received(1).UpdateAsync(user);
        }

        [Fact(DisplayName = "Update User Not Found Should Throw Exception")]
        public async Task Update_WhenUserDoesNotExist_ShouldThrowNotFound()
        {
            var userId = Guid.NewGuid();
            var dto = new UsuarioUpdateDTO("Novo Nome", ETipoUsuario.Administrador);
            _userManager.FindByIdAsync(userId.ToString()).Returns((Usuario?)null);

            Func<Task> action = async () => await _usuarioService.Update(userId, dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "Delete User Successfully")]
        public async Task Delete_WhenUserExists_ShouldCallDelete()
        {
            var userId = Guid.NewGuid();
            var user = new Usuario("Test", Domain.Clientes.ValueObjects.Email.Create("test@test.com"), ETipoUsuario.Mecanico);
            _userManager.FindByIdAsync(userId.ToString()).Returns(user);
            _userManager.DeleteAsync(user).Returns(IdentityResult.Success);

            await _usuarioService.Delete(userId);

            await _userManager.Received(1).DeleteAsync(user);
        }
    }
}
