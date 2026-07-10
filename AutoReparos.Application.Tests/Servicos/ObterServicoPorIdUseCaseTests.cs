using AutoReparos.Application.Servicos.UseCases;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Servicos
{
    public class ObterServicoPorIdUseCaseTests
    {
        private readonly IServicoRepository _repository;
        private readonly ObterServicoPorIdUseCase _useCase;

        public ObterServicoPorIdUseCaseTests()
        {
            _repository = Substitute.For<IServicoRepository>();
            _useCase = new ObterServicoPorIdUseCase(_repository);
        }

        [Fact(DisplayName = "GetById When Servico Exists Should Return Dto")]
        public async Task GetById_WhenServicoExists_ShouldReturnDto()
        {
            var servico = new Servico("Troca de óleo", "Descrição", 150m);
            _repository.GetById(servico.Id).Returns(servico);

            var result = await _useCase.ExecuteAsync(servico.Id);

            result.Should().NotBeNull();
            result!.Nome.Should().Be(servico.Nome);
        }

        [Fact(DisplayName = "GetById When Servico Does Not Exist Should Return Null")]
        public async Task GetById_WhenServicoDoesNotExist_ShouldReturnNull()
        {
            var id = Guid.NewGuid();
            _repository.GetById(id).Returns((Servico?)null);

            var result = await _useCase.ExecuteAsync(id);

            result.Should().BeNull();
        }
    }
}
