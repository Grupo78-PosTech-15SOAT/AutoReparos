using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.UseCases;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Servicos
{
    public class ListarServicosUseCaseTests
    {
        private readonly IServicoRepository _repository;
        private readonly ListarServicosUseCase _useCase;

        public ListarServicosUseCaseTests()
        {
            _repository = Substitute.For<IServicoRepository>();
            _useCase = new ListarServicosUseCase(_repository);
        }

        [Fact(DisplayName = "GetAll Should Return Paged Result")]
        public async Task GetAll_ShouldReturnPagedResult()
        {
            var servicos = new List<Servico> { new("Troca de óleo", "Descrição", 150m) };
            var request = new ServicoPagedRequest { PageNumber = 1, PageSize = 10 };

            _repository.GetAll(request.Nome, request.Skip, request.PageSize).Returns((servicos, servicos.Count));

            var result = await _useCase.ExecuteAsync(request);

            result.Items.Should().HaveCount(1);
            result.TotalItems.Should().Be(1);
        }
    }
}
