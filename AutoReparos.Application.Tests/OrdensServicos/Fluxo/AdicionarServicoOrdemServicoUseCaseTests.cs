using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.UseCases.Fluxo;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace AutoReparos.Application.Tests.OrdensServicos.Fluxo
{
    public class AdicionarServicoOrdemServicoUseCaseTests
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly AdicionarServicoOrdemServicoUseCase _useCase;

        public AdicionarServicoOrdemServicoUseCaseTests()
        {
            _repository = Substitute.For<IOrdemServicoRepository>();
            _useCase = new AdicionarServicoOrdemServicoUseCase(_repository);
        }

        [Fact(DisplayName = "AdicionarServico When OrdemServico Exists Should Add Item")]
        public async Task AdicionarServico_WhenOrdemServicoExists_ShouldAddItem()
        {
            var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), "obs");
            var dto = new AdicionarServicoDto(Guid.NewGuid(), 150m);

            _repository.GetById(os.Id).Returns(os);

            await _useCase.ExecuteAsync(os.Id, dto);

            os.Servicos.Should().ContainSingle(s => s.ServicoId == dto.ServicoId);
            await _repository.Received(1).Update(os);
        }

        [Fact(DisplayName = "AdicionarServico When OrdemServico Does Not Exist Should Throw NotFoundException")]
        public async Task AdicionarServico_WhenOrdemServicoDoesNotExist_ShouldThrowNotFoundException()
        {
            var id = Guid.NewGuid();
            var dto = new AdicionarServicoDto(Guid.NewGuid(), 150m);
            _repository.GetById(id).Returns((OrdemServico?)null);

            Func<Task> action = async () => await _useCase.ExecuteAsync(id, dto);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }
}
