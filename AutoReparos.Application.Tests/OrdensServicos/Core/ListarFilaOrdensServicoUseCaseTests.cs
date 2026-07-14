using AutoReparos.Application.OrdensServicos.UseCases.Core;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.OrdensServicos.Core
{
    public class ListarFilaOrdensServicoUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly ListarFilaOrdensServicoUseCase _useCase;

        public ListarFilaOrdensServicoUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _useCase = new ListarFilaOrdensServicoUseCase(_repository);
        }

        [Fact(DisplayName = "GetFila Should Return Paged Result")]
        public async Task GetFila_ShouldReturnPagedResult()
        {
            var ordens = new List<OrdemServico> { new(Guid.NewGuid(), Guid.NewGuid(), "obs") };
            var request = new PagedRequest { PageNumber = 1, PageSize = 10 };

            _repository.GetFila(request.Skip, request.PageSize).Returns((ordens, ordens.Count));

            var result = await _useCase.ExecuteAsync(request);

            result.Items.Should().HaveCount(1);
            result.TotalItems.Should().Be(1);
        }
    }
}
