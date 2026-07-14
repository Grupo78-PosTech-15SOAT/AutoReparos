using AutoReparos.Application.Veiculos.DTOs.Request;
using AutoReparos.Application.Veiculos.UseCases;
using AutoReparos.Domain.Veiculos.Entities;
using AutoReparos.Domain.Veiculos.Repositories;
using AutoReparos.Domain.Veiculos.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Veiculos
{
    public class ListarVeiculosUseCaseTests
    {
        private readonly IVeiculoRepository _repository;
        private readonly ListarVeiculosUseCase _useCase;

        public ListarVeiculosUseCaseTests()
        {
            _repository = Substitute.For<IVeiculoRepository>();
            _useCase = new ListarVeiculosUseCase(_repository);
        }

        [Fact(DisplayName = "GetAll Should Return Paged Result")]
        public async Task GetAll_ShouldReturnPagedResult()
        {
            var veiculos = new List<Veiculo>
            {
                new(Guid.NewGuid(), "Chevrolet", "Onix", 2020, 2021, new Placa("ABC1D23"), new Chassi("9BD111060T5002156"), new Renavam("00123456789"))
            };

            var request = new VeiculoPagedRequest { PageNumber = 1, PageSize = 10 };
            _repository.GetAll(request.ClienteId, request.Skip, request.PageSize).Returns((veiculos, veiculos.Count));

            var result = await _useCase.ExecuteAsync(request);

            result.Items.Should().HaveCount(1);
            result.TotalItems.Should().Be(1);
        }
    }
}
