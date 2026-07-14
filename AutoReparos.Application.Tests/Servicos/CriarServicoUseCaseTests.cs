using AutoReparos.Application.Servicos.DTOs.Request;
using AutoReparos.Application.Servicos.UseCases;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Servicos.Repositories;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.Servicos
{
    public class CriarServicoUseCaseTests
    {
        private readonly IServicoRepository _repository;
        private readonly CriarServicoUseCase _useCase;

        public CriarServicoUseCaseTests()
        {
            _repository = Substitute.For<IServicoRepository>();
            _useCase = new CriarServicoUseCase(_repository);
        }

        [Fact(DisplayName = "Create Servico With Valid Data Should Return Dto")]
        public async Task Create_WithValidData_ShouldReturnDto()
        {
            var dto = new CriarServicoDto("Troca de óleo", "Troca de óleo do motor", 150m);

            var result = await _useCase.ExecuteAsync(dto);

            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            result.ValorTabelado.Should().Be(dto.ValorTabelado);
            await _repository.Received(1).Create(Arg.Any<Servico>());
        }
    }
}
