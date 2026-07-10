using AutoReparos.Application.Usuarios.UseCases;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Usuarios.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Usuarios
{
    public class ObterUsuarioPorIdUseCaseTests
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ObterUsuarioPorIdUseCase _useCase;

        public ObterUsuarioPorIdUseCaseTests()
        {
            _usuarioRepository = Substitute.For<IUsuarioRepository>();
            _useCase = new ObterUsuarioPorIdUseCase(_usuarioRepository);
        }

        [Fact(DisplayName = "Get Usuario By Id Successfully")]
        public async Task GetById_WhenUserExists_ShouldReturnDto()
        {
            var usuarioId = Guid.NewGuid();
            var usuario = new Usuario("Test", "test@test.com", ETipoUsuario.Mecanico);
            _usuarioRepository.GetByIdAsync(usuarioId).Returns(usuario);

            var result = await _useCase.ExecuteAsync(usuarioId);

            result.Should().NotBeNull();
            result.NomeCompleto.Should().Be(usuario.NomeCompleto);
        }

        [Fact(DisplayName = "Get Usuario By Id Not Found Should Throw Exception")]
        public async Task GetById_WhenUserDoesNotExist_ShouldThrowNotFound()
        {
            var usuarioId = Guid.NewGuid();
            _usuarioRepository.GetByIdAsync(usuarioId).Returns((Usuario?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(usuarioId);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
