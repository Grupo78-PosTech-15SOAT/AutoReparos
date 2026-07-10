using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.UseCases;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.OrdensServicos
{
    public class ConsultarOrdensServicoPorDocumentoOuPlacaUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly ConsultarOrdensServicoPorDocumentoOuPlacaUseCase _useCase;

        public ConsultarOrdensServicoPorDocumentoOuPlacaUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _useCase = new ConsultarOrdensServicoPorDocumentoOuPlacaUseCase(_repository);
        }

        [Fact(DisplayName = "GetByDocumentoOuPlaca Should Return Paged Result")]
        public async Task GetByDocumentoOuPlaca_ShouldReturnPagedResult()
        {
            var ordens = new List<OrdemServico> { new(Guid.NewGuid(), Guid.NewGuid(), "obs") };
            var request = new OrdemServicoConsultaPagedRequest { Placa = "ABC1D23", PageNumber = 1, PageSize = 10 };

            _repository.GetByDocumentoOuPlaca(request.Documento, request.Placa, request.Skip, request.PageSize)
                .Returns((ordens, ordens.Count));

            var result = await _useCase.ExecuteAsync(request);

            result.Items.Should().HaveCount(1);
            result.TotalItems.Should().Be(1);
        }
    }
}
