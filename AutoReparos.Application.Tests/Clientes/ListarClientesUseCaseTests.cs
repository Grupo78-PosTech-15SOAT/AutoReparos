using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.UseCases;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Clientes
{
    public class ListarClientesUseCaseTests
    {
        private readonly IClienteRepository _repository;
        private readonly ListarClientesUseCase _useCase;

        public ListarClientesUseCaseTests()
        {
            _repository = Substitute.For<IClienteRepository>();
            _useCase = new ListarClientesUseCase(_repository);
        }

        [Fact(DisplayName = "GetAll Should Return Paged Result")]
        public async Task GetAll_ShouldReturnPagedResult()
        {
            var clientes = new List<Cliente>
            {
                new("João Silva", "52998224725", "11999999999", "joao@teste.com"),
                new("Maria Souza", "11144477735", "11988888888", "maria@teste.com")
            };

            var request = new ClientePagedRequest { PageNumber = 1, PageSize = 10 };
            _repository.GetAll(request.Nome, request.Skip, request.PageSize).Returns((clientes, clientes.Count));

            var result = await _useCase.ExecuteAsync(request);

            result.Items.Should().HaveCount(2);
            result.TotalItems.Should().Be(2);
        }
    }
}
