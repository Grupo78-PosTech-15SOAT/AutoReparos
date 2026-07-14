using AutoReparos.Application.Insumos.DTOs.Request;
using AutoReparos.Application.Insumos.UseCases;
using AutoReparos.Domain.Insumos.Entities;
using AutoReparos.Domain.Insumos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Insumos
{
    public class ListarInsumosUseCaseTests
    {
        private readonly IInsumoRepository _repository;
        private readonly ListarInsumosUseCase _useCase;

        public ListarInsumosUseCaseTests()
        {
            _repository = Substitute.For<IInsumoRepository>();
            _useCase = new ListarInsumosUseCase(_repository);
        }

        [Fact(DisplayName = "GetAll Should Return Paged Result")]
        public async Task GetAll_ShouldReturnPagedResult()
        {
            var insumos = new List<Insumo> { new("Óleo 5W30", "Óleo sintético", 45m, 10) };
            var request = new InsumoPagedRequest { PageNumber = 1, PageSize = 10 };

            _repository.GetAll(request.Nome, request.Skip, request.PageSize).Returns((insumos, insumos.Count));

            var result = await _useCase.ExecuteAsync(request);

            result.Items.Should().HaveCount(1);
            result.TotalItems.Should().Be(1);
        }
    }
}
