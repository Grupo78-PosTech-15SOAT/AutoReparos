using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.Servicos.DTOs.Response;
using AutoReparos.Application.Shared;
using AutoReparos.Application.Shared.Interfaces;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Insumos.Repositories;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Servicos.Repositories;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Exceptions;
using AutoReparos.Domain.Veiculos.Repositories;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace AutoReparos.Application.OrdensServicos.Services
{
    public class OrdemServicoService : IOrdemServicoService
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly IInsumoRepository _insumoRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly IServicoRepository _servicoRepository;
        private readonly IAprovacaoTokenService _aprovacaoTokenService;
        private readonly IClienteRepository _clienteRepository;
        private readonly ILogger<OrdemServicoService> _logger;

        public OrdemServicoService(
            IOrdemServicoRepository repository,
            IVeiculoRepository veiculoRepository,
            IInsumoRepository insumoRepository,
            INotificacaoService notificacaoService,
            IServicoRepository servicoRepository,
            IAprovacaoTokenService aprovacaoTokenService,
            IClienteRepository clienteRepository,
            ILogger<OrdemServicoService> logger)
        {
            _repository = repository;
            _veiculoRepository = veiculoRepository;
            _insumoRepository = insumoRepository;
            _notificacaoService = notificacaoService;
            _servicoRepository = servicoRepository;
            _aprovacaoTokenService = aprovacaoTokenService;
            _clienteRepository = clienteRepository;
            _logger = logger;
        }

        public async Task<OrdemServicoDto> Create(CriarOrdemServicoDto dto)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var veiculo = await _veiculoRepository.GetById(dto.VeiculoId)
                ?? throw new NotFoundException(ErrorMessages.VeiculoNotFound);

            if (veiculo.ClienteId != dto.ClienteId)
                throw new InvalidVeiculoException("Veículo não pertence ao cliente informado.");

            var cliente = await _clienteRepository.GetById(dto.ClienteId)
                ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);

            var ordemServico = new OrdemServico(dto.ClienteId, dto.VeiculoId, dto.Observacao);

            if (dto.Servicos != null)
            {
                foreach (var servicoDto in dto.Servicos)
                {
                    var servico = await _servicoRepository.GetById(servicoDto.ServicoId)
                        ?? throw new NotFoundException(ErrorMessages.ServicoNotFound);

                    var item = new OrdemServicoServico(ordemServico.Id, servicoDto.ServicoId, servicoDto.ValorCobrado);
                    ordemServico.AdicionarServico(item);
                }
            }

            if (dto.Insumos != null)
            {
                foreach (var insumoDto in dto.Insumos)
                {
                    decimal valorUnitario;

                    if (insumoDto.Origem == EOrigemInsumo.Estoque)
                    {
                        if (!insumoDto.InsumoId.HasValue)
                            throw new ValidationException("InsumoId é obrigatório para insumos de estoque.");

                        var insumo = await _insumoRepository.GetById(insumoDto.InsumoId.Value)
                            ?? throw new NotFoundException(ErrorMessages.InsumoNotFound);

                        insumo.RemoverEstoque(insumoDto.Quantidade);
                        await _insumoRepository.Update(insumo);

                        valorUnitario = insumoDto.ValorUnitario ?? insumo.Valor;
                    }
                    else
                    {
                        if (insumoDto.ValorUnitario == null)
                            throw new ValidationException("Valor unitário é obrigatório para insumos de compra específica.");

                        valorUnitario = insumoDto.ValorUnitario.Value;
                    }

                    var item = new OrdemServicoInsumo(ordemServico.Id, insumoDto.InsumoId, insumoDto.Descricao, valorUnitario, insumoDto.Quantidade, insumoDto.Origem);
                    ordemServico.AdicionarInsumo(item);
                }
            }

            await _repository.Create(ordemServico);

            scope.Complete();

            try
            {
                await _notificacaoService.EnviarAtualizacaoStatus(cliente.Email.Endereco, cliente.Nome, ordemServico.Id, "Nenhum", ordemServico.Status.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar e-mail de notificação de criação de OS {Id}", ordemServico.Id);
            }

            return ToDTO(ordemServico);
        }

        public async Task<PagedResult<OrdemServicoDto>> GetAll(OrdemServicoPagedRequest request)
        {
            var (items, total) = await _repository.GetAll(request.ClienteId, request.VeiculoId, request.Status, request.Skip, request.PageSize);
            return new PagedResult<OrdemServicoDto>(items.Select(ToDTO), total, request.PageNumber, request.PageSize);
        }

        public async Task<PagedResult<OrdemServicoDto>> GetFila(PagedRequest request)
        {
            var (items, total) = await _repository.GetFila(request.Skip, request.PageSize);
            return new PagedResult<OrdemServicoDto>(items.Select(ToDTO), total, request.PageNumber, request.PageSize);
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

            var statusAnterior = os.Status.ToString();
            os.IniciarDiagnostico();
            await _repository.Update(os);

            try
            {
                var cliente = await _clienteRepository.GetById(os.ClienteId)
                    ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);
                await _notificacaoService.EnviarAtualizacaoStatus(cliente.Email.Endereco, cliente.Nome, os.Id, statusAnterior, os.Status.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar e-mail de notificação de status de OS {Id}", os.Id);
            }
        }

        public async Task AguardarAprovacao(Guid id)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            os.AguardarAprovacao();
            await _repository.Update(os);

            var token = _aprovacaoTokenService.GerarToken(os.Id);

            var servicosDescricao = new List<ServicoDto>();

            foreach (var item in os.Servicos)
            {
                var servico = await _servicoRepository.GetById(item.ServicoId);

                if (servico is not null)
                {
                    servicosDescricao.Add(new ServicoDto(
                        servico.Id, servico.Nome, servico.Descricao, item.ValorCobrado, servico.CriadoEm, servico.AtualizadoEm));
                }
            }

            try
            {
                var cliente = await _clienteRepository.GetById(os.ClienteId)
                    ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);
                await _notificacaoService.EnviarOrcamento(cliente.Email.Endereco, cliente.Nome, token, os.ValorTotal, servicosDescricao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar e-mail de orçamento de OS {Id}", os.Id);
            }
        }

        public async Task Aprovar(string token)
        {
            var ordemServicoId = _aprovacaoTokenService.ValidarToken(token);
            var os = await _repository.GetById(ordemServicoId)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            var statusAnterior = os.Status.ToString();
            os.Aprovar();
            await _repository.Update(os);

            try
            {
                var cliente = await _clienteRepository.GetById(os.ClienteId)
                    ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);
                await _notificacaoService.EnviarAtualizacaoStatus(cliente.Email.Endereco, cliente.Nome, os.Id, statusAnterior, os.Status.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar e-mail de aprovação de OS {Id}", os.Id);
            }
        }

        public async Task Recusar(string token)
        {
            var ordemServicoId = _aprovacaoTokenService.ValidarToken(token);
            var os = await _repository.GetById(ordemServicoId)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            var statusAnterior = os.Status.ToString();
            os.Recusar();
            await _repository.Update(os);

            try
            {
                var cliente = await _clienteRepository.GetById(os.ClienteId)
                    ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);
                await _notificacaoService.EnviarAtualizacaoStatus(cliente.Email.Endereco, cliente.Nome, os.Id, statusAnterior, os.Status.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar e-mail de recusa de OS {Id}", os.Id);
            }
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

            var statusAnterior = os.Status.ToString();
            os.ConcluirServico(ordemServicoServicoId);
            await _repository.Update(os);

            if (statusAnterior != os.Status.ToString())
            {
                try
                {
                    var cliente = await _clienteRepository.GetById(os.ClienteId)
                        ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);
                    await _notificacaoService.EnviarAtualizacaoStatus(cliente.Email.Endereco, cliente.Nome, os.Id, statusAnterior, os.Status.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao enviar e-mail de conclusão de OS {Id}", os.Id);
                }
            }
        }

        public async Task Entregar(Guid id)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException(ErrorMessages.OrdemServicoNotFound);

            var statusAnterior = os.Status.ToString();
            os.Entregar();
            await _repository.Update(os);

            try
            {
                var cliente = await _clienteRepository.GetById(os.ClienteId)
                    ?? throw new NotFoundException(ErrorMessages.ClienteNotFound);
                await _notificacaoService.EnviarAtualizacaoStatus(cliente.Email.Endereco, cliente.Nome, os.Id, statusAnterior, os.Status.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar e-mail de entrega de OS {Id}", os.Id);
            }
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
            os.Servicos.Select(s => new OrdemServicoServicoPublicoDto(
                s.Id, s.Status.ToString(), s.IniciadoEm, s.ConcluidoEm)),
            os.Insumos.Select(p => new OrdemServicoInsumoPublicoDto(
                p.Id, p.Descricao, p.Quantidade))
        );
    }
}
