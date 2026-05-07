using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Exceptions;
using AutoReparos.Domain.Veiculos.Repositories;
using System.ComponentModel.DataAnnotations;

namespace AutoReparos.Application.OrdensServicos.Services
{
    public class OrdemServicoService : IOrdemServicoService
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly IInsumoRepository _insumoRepository;
        private readonly INotificacaoService _notificacaoService;

        public OrdemServicoService(
            IOrdemServicoRepository repository,
            IVeiculoRepository veiculoRepository,
            IInsumoRepository insumoRepository,
            INotificacaoService notificacaoService)
        {
            _repository = repository;
            _veiculoRepository = veiculoRepository;
            _insumoRepository = insumoRepository;
            _notificacaoService = notificacaoService;
        }

        public async Task<OrdemServicoDto> Create(CriarOrdemServicoDto dto)
        {
            var veiculo = await _veiculoRepository.GetById(dto.VeiculoId)
                ?? throw new NotFoundException(ErrorMessages.VeiculoNotFound);

            if (veiculo.ClienteId != dto.ClienteId)
                throw new InvalidVeiculoException("Veículo não pertence ao cliente informado.");

            var ordemServico = new OrdemServico(dto.ClienteId, dto.VeiculoId, dto.Observacao);
            await _repository.Create(ordemServico);
            return ToDTO(ordemServico);
        }

        public async Task<OrdemServicoDetalheDto?> GetById(Guid id)
        {
            var os = await _repository.GetById(id);
            return os is null ? null : ToDetalheDto(os);
        }

        public async Task<OrdemServicoPublicoDetalheDto?> GetPublicById(Guid id)
        {
            var os = await _repository.GetById(id);
            return os is null ? null : ToPublicDetalheDto(os);
        }

        public async Task<PagedResult<OrdemServicoDto>> GetAll(OrdemServicoPagedRequest request)
        {
            var (items, total) = await _repository.GetAll(request.ClienteId, request.VeiculoId, request.Status, request.Skip, request.PageSize);
            return new PagedResult<OrdemServicoDto>(items.Select(ToDTO), total, request.PageNumber, request.PageSize);
        }

        public async Task<PagedResult<OrdemServicoPublicoDto>> GetByDocumentoOuPlaca(OrdemServicoConsultaPagedRequest request)
        {
            var (items, total) = await _repository.GetByDocumentoOuPlaca(request.Documento, request.Placa, request.Skip, request.PageSize);
            return new PagedResult<OrdemServicoPublicoDto>(items.Select(ToPublicDTO), total, request.PageNumber, request.PageSize);
        }

        public async Task AdicionarServico(Guid id, AdicionarServicoDto dto)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            var item = new OrdemServicoServico(id, dto.ServicoId, dto.ValorCobrado);
            os.AdicionarServico(item);
            await _repository.Update(os);
        }

        public async Task AdicionarInsumo(Guid id, AdicionarInsumoDto dto)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            decimal valorUnitario;

            if (dto.Origem == EOrigemInsumo.Estoque)
            {
                var insumo = await _insumoRepository.GetById(dto.InsumoId!.Value)
                    ?? throw new NotFoundException(ErrorMessages.InsumoNotFound);

                insumo.RemoverEstoque(dto.Quantidade);
                await _insumoRepository.Update(insumo);

                valorUnitario = dto.ValorUnitario ?? insumo.Valor;
            }
            else
            {
                if (dto.ValorUnitario == null)
                    throw new ValidationException("Valor unitário é obrigatório para insumos de compra específica.");

                valorUnitario = dto.ValorUnitario.Value;
            }

            var item = new OrdemServicoInsumo(id, dto.InsumoId, dto.Descricao, valorUnitario, dto.Quantidade, dto.Origem);
            os.AdicionarInsumo(item);
            await _repository.Update(os);
        }

        public async Task IniciarDiagnostico(Guid id)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            os.IniciarDiagnostico();
            await _repository.Update(os);
        }

        public async Task AguardarAprovacao(Guid id)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            os.AguardarAprovacao();
            await _repository.Update(os);

            // mock de envio de email para cliente
            await _notificacaoService.EnviarOrcamento(os.Id, os.ValorTotal);
        }

        public async Task Aprovar(Guid id)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            os.Aprovar();
            await _repository.Update(os);
        }

        public async Task IniciarServico(Guid ordemServicoId, Guid ordemServicoServicoId)
        {
            var os = await _repository.GetById(ordemServicoId)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            os.IniciarServico(ordemServicoServicoId);
            await _repository.Update(os);
        }

        public async Task ConcluirServico(Guid ordemServicoId, Guid ordemServicoServicoId)
        {
            var os = await _repository.GetById(ordemServicoId)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            os.ConcluirServico(ordemServicoServicoId);
            await _repository.Update(os);
        }

        public async Task Entregar(Guid id)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            os.Entregar();
            await _repository.Update(os);
        }

        private static OrdemServicoDto ToDTO(OrdemServico os) => new(
            os.Id, os.ClienteId, os.VeiculoId, os.Status.ToString(),
            os.Observacao, os.ValorTotal, os.CriadoEm,
            os.IniciadoEm, os.FinalizadoEm, os.EntregueEm
        );

        private static OrdemServicoPublicoDto ToPublicDTO(OrdemServico os) => new(
            os.Id, os.Status.ToString(), os.Observacao, os.CriadoEm,
            os.IniciadoEm, os.FinalizadoEm, os.EntregueEm
        );

        private static OrdemServicoDetalheDto ToDetalheDto(OrdemServico os) => new(
            os.Id, os.ClienteId, os.VeiculoId, os.Status.ToString(),
            os.Observacao, os.ValorTotal, os.CriadoEm,
            os.IniciadoEm, os.FinalizadoEm, os.EntregueEm,
            os.Servicos.Select(s => new OrdemServicoServicoDto(
                s.Id, s.ServicoId, s.ValorCobrado, s.Status.ToString(),
                s.IniciadoEm, s.ConcluidoEm, s.TempoExecucao)),
            os.Insumos.Select(p => new OrdemServicoInsumoDto(
                p.Id, p.InsumoId, p.Descricao, p.ValorUnitario,
                p.Quantidade, p.ValorTotal, p.Origem.ToString()))
        );

        private static OrdemServicoPublicoDetalheDto ToPublicDetalheDto(OrdemServico os) => new(
            os.Id, os.Status.ToString(), os.Observacao, os.CriadoEm,
            os.IniciadoEm, os.FinalizadoEm, os.EntregueEm,
            os.Servicos.Select(s => new OrdemServicoServicoPublicoDTO(
                s.Id, s.Status.ToString(), s.IniciadoEm, s.ConcluidoEm)),
            os.Insumos.Select(p => new OrdemServicoInsumoPublicoDTO(
                p.Id, p.Descricao, p.Quantidade))
        );
    }
}
