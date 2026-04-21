using AutoReparos.Application.OrdensServicos.DTOs.Request;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Application.OrdensServicos.Services.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Repositories;
using AutoReparos.Domain.Pecas.Repositories;
using AutoReparos.Domain.Shared.Exceptions;
using AutoReparos.Domain.Veiculos.Exceptions;
using AutoReparos.Domain.Veiculos.Repositories;

namespace AutoReparos.Application.OrdensServicos.Services
{
    public class OrdemServicoService : IOrdemServicoService
    {
        private readonly IOrdemServicoRepository _repository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly IPecaRepository _pecaRepository;

        public OrdemServicoService(
            IOrdemServicoRepository repository,
            IClienteRepository clienteRepository,
            IVeiculoRepository veiculoRepository,
            IPecaRepository pecaRepository)
        {
            _repository = repository;
            _clienteRepository = clienteRepository;
            _veiculoRepository = veiculoRepository;
            _pecaRepository = pecaRepository;
        }

        public async Task<OrdemServicoDTO> Create(CriarOrdemServicoDTO dto)
        {
            var cliente = await _clienteRepository.GetById(dto.ClienteId)
                ?? throw new NotFoundException("Cliente não encontrado.");

            var veiculo = await _veiculoRepository.GetById(dto.VeiculoId)
                ?? throw new NotFoundException("Veículo não encontrado.");

            if (veiculo.ClienteId != dto.ClienteId)
                throw new InvalidVeiculoException("Veículo não pertence ao cliente informado.");

            var ordemServico = new OrdemServico(dto.ClienteId, dto.VeiculoId, dto.Observacao);
            await _repository.Create(ordemServico);
            return ToDTO(ordemServico);
        }

        public async Task<OrdemServicoDetalheDTO?> GetById(Guid id)
        {
            var os = await _repository.GetById(id);
            return os is null ? null : ToDetalheDto(os);
        }

        public async Task<PagedResult<OrdemServicoDTO>> GetAll(
            Guid? clienteId, Guid? veiculoId, EStatusOrdemServico? status, int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            var (items, total) = await _repository.GetAll(clienteId, veiculoId, status, skip, pageSize);
            return new PagedResult<OrdemServicoDTO>(items.Select(ToDTO), total, pageNumber, pageSize);
        }

        public async Task AdicionarServico(Guid id, AdicionarServicoDTO dto)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException("Ordem de serviço não encontrada.");

            var item = new OrdemServicoServico(id, dto.ServicoId, dto.ValorCobrado);
            os.AdicionarServico(item);
            await _repository.Update(os);
        }

        public async Task AdicionarPeca(Guid id, AdicionarPecaDTO dto)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException("Ordem de serviço não encontrada.");

            if (dto.Origem == EOrigemPeca.Estoque)
            {
                var peca = await _pecaRepository.GetById(dto.PecaId!.Value)
                    ?? throw new NotFoundException("Peça não encontrada.");

                peca.RemoverEstoque(dto.Quantidade);
                await _pecaRepository.Update(peca);
            }

            var item = new OrdemServicoPeca(id, dto.PecaId, dto.Descricao, dto.ValorUnitario, dto.Quantidade, dto.Origem);
            os.AdicionarPeca(item);
            await _repository.Update(os);
        }

        public async Task IniciarDiagnostico(Guid id)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException("Ordem de serviço não encontrada.");

            os.IniciarDiagnostico();
            await _repository.Update(os);
        }

        public async Task AguardarAprovacao(Guid id)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException("Ordem de serviço não encontrada.");

            os.AguardarAprovacao();
            await _repository.Update(os);
        }

        public async Task Aprovar(Guid id)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException("Ordem de serviço não encontrada.");

            os.Aprovar();
            await _repository.Update(os);
        }

        public async Task ConcluirServico(Guid ordemServicoId, Guid ordemServicoServicoId)
        {
            var os = await _repository.GetById(ordemServicoId)
                ?? throw new NotFoundException("Ordem de serviço não encontrada.");

            os.ConcluirServico(ordemServicoServicoId);
            await _repository.Update(os);
        }

        public async Task Entregar(Guid id)
        {
            var os = await _repository.GetById(id)
                ?? throw new NotFoundException("Ordem de serviço não encontrada.");

            os.Entregar();
            await _repository.Update(os);
        }

        private static OrdemServicoDTO ToDTO(OrdemServico os) => new(
            os.Id, os.ClienteId, os.VeiculoId, os.Status.ToString(),
            os.Observacao, os.ValorTotal, os.CriadoEm,
            os.IniciadoEm, os.FinalizadoEm, os.EntregueEm
        );

        private static OrdemServicoDetalheDTO ToDetalheDto(OrdemServico os) => new(
            os.Id, os.ClienteId, os.VeiculoId, os.Status.ToString(),
            os.Observacao, os.ValorTotal, os.CriadoEm,
            os.IniciadoEm, os.FinalizadoEm, os.EntregueEm,
            os.Servicos.Select(s => new OrdemServicoServicoDTO(
                s.Id, s.ServicoId, s.ValorCobrado, s.Status.ToString(),
                s.IniciadoEm, s.ConcluidoEm, s.TempoExecucao)),
            os.Pecas.Select(p => new OrdemServicoPecaDTO(
                p.Id, p.PecaId, p.Descricao, p.ValorUnitario,
                p.Quantidade, p.ValorTotal, p.Origem.ToString()))
        );
    }
}
