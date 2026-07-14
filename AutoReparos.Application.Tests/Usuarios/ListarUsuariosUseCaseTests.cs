using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.UseCases;
using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using AutoReparos.Domain.Usuarios.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Usuarios
{
    public class ListarUsuariosUseCaseTests
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ListarUsuariosUseCase _useCase;

        public ListarUsuariosUseCaseTests()
        {
            _usuarioRepository = Substitute.For<IUsuarioRepository>();
            _useCase = new ListarUsuariosUseCase(_usuarioRepository);
        }

        [Fact(DisplayName = "List Usuarios Should Return Paged Result")]
        public async Task GetAll_ShouldReturnPagedResult()
        {
            var usuarios = new List<Usuario>
            {
                new("Test", "test@test.com", ETipoUsuario.Mecanico)
            };
            var request = new UsuarioPagedRequest { PageNumber = 1, PageSize = 10 };

            _usuarioRepository.GetAllAsync(request.Nome, request.Skip, request.PageSize)
                .Returns((usuarios, usuarios.Count));

            var result = await _useCase.ExecuteAsync(request);

            result.Items.Should().HaveCount(1);
            result.TotalItems.Should().Be(1);
        }
    }
}
