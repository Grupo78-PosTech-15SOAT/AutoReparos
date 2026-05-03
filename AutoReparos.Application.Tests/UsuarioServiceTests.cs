using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.Services;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Usuarios.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests
{
    public class UsuarioServiceTests
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly UsuarioService _usuarioService;

        public UsuarioServiceTests()
        {
            _usuarioRepository = Substitute.For<IUsuarioRepository>();
            _usuarioService = new UsuarioService(_usuarioRepository);
        }

        [Fact(DisplayName = "Create Usuario Successfully")]
        public async Task Create_WithValidData_ShouldReturnDto()
        {
            var dto = new UsuarioCreateDTO("Novo Usuario", "novo@test.com", "Pass123!", ETipoUsuario.Atendente);
            
            var result = await _usuarioService.Create(dto);

            result.Should().NotBeNull();
            result.NomeCompleto.Should().Be(dto.NomeCompleto);
            result.Email.Should().Be(dto.Email);
            await _usuarioRepository.Received(1).CreateAsync(Arg.Any<Usuario>(), dto.Password);
        }

        [Fact(DisplayName = "Get Usuario By Id Successfully")]
        public async Task GetById_WhenUserExists_ShouldReturnDto()
        {
            var usuarioId = Guid.NewGuid();
            var usuario = new Usuario("Test", "test@test.com", ETipoUsuario.Mecanico);
            _usuarioRepository.GetByIdAsync(usuarioId).Returns(usuario);

            var result = await _usuarioService.GetById(usuarioId);

            result.Should().NotBeNull();
            result.NomeCompleto.Should().Be(usuario.NomeCompleto);
        }

        [Fact(DisplayName = "Get Usuario By Id Not Found Should Throw Exception")]
        public async Task GetById_WhenUserDoesNotExist_ShouldThrowNotFound()
        {
            var usuarioId = Guid.NewGuid();
            _usuarioRepository.GetByIdAsync(usuarioId).Returns((Usuario?)null);

            Func<Task> action = async () => await _usuarioService.GetById(usuarioId);

            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "Update Usuario Successfully")]
        public async Task Update_WhenUserExists_ShouldUpdateAndCallUpdateAsync()
        {
            var usuarioId = Guid.NewGuid();
            var usuario = new Usuario("Antigo", "test@test.com", ETipoUsuario.Atendente);
            var dto = new UsuarioUpdateDTO("Novo Nome", ETipoUsuario.Administrador);

            _usuarioRepository.GetByIdAsync(usuarioId).Returns(usuario);

            await _usuarioService.Update(usuarioId, dto);

            usuario.NomeCompleto.Should().Be(dto.NomeCompleto);
            usuario.Tipo.Should().Be(dto.Tipo);
            await _usuarioRepository.Received(1).UpdateAsync(usuario);
        }

        [Fact(DisplayName = "Delete Usuario Successfully")]
        public async Task Delete_WhenUserExists_ShouldCallDelete()
        {
            var usuarioId = Guid.NewGuid();
            var usuario = new Usuario("Test", "test@test.com", ETipoUsuario.Mecanico);
            _usuarioRepository.GetByIdAsync(usuarioId).Returns(usuario);

            await _usuarioService.Delete(usuarioId);

            await _usuarioRepository.Received(1).DeleteAsync(usuario);
        }
    }
}
