using AutoReparos.Application.Usuarios.UseCases;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Usuarios.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Usuarios
{
    public class ExcluirUsuarioUseCaseTests
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ExcluirUsuarioUseCase _useCase;

        public ExcluirUsuarioUseCaseTests()
        {
            _usuarioRepository = Substitute.For<IUsuarioRepository>();
            _useCase = new ExcluirUsuarioUseCase(_usuarioRepository);
        }

        [Fact(DisplayName = "Delete Usuario Successfully")]
        public async Task Delete_WhenUserExists_ShouldCallDelete()
        {
            var usuarioId = Guid.NewGuid();
            var usuario = new Usuario("Test", "test@test.com", ETipoUsuario.Mecanico);
            _usuarioRepository.GetByIdAsync(usuarioId).Returns(usuario);

            await _useCase.ExecuteAsync(usuarioId);

            await _usuarioRepository.Received(1).DeleteAsync(usuario);
        }

        [Fact(DisplayName = "Delete Usuario Not Found Should Throw Exception")]
        public async Task Delete_WhenUserDoesNotExist_ShouldThrowNotFound()
        {
            var usuarioId = Guid.NewGuid();
            _usuarioRepository.GetByIdAsync(usuarioId).Returns((Usuario?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(usuarioId);

            await action.Should().ThrowAsync<NotFoundException>();
            await _usuarioRepository.DidNotReceive().DeleteAsync(Arg.Any<Usuario>());
        }
    }
}
