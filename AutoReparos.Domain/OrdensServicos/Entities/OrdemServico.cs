using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Exceptions;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Domain.OrdensServicos.Entities
{
    public class OrdemServico : Entity
    {
        private readonly List<OrdemServicoServico> _servicos = [];
        private readonly List<OrdemServicoInsumo> _insumos = [];

        public Guid ClienteId { get; private set; }
        public Guid VeiculoId { get; private set; }
        public EStatusOrdemServico Status { get; private set; }
        public string? Observacao { get; private set; }
        public DateTime CriadoEm { get; }
        public DateTime? IniciadoEm { get; private set; }
        public DateTime? FinalizadoEm { get; private set; }
        public DateTime? EntregueEm { get; private set; }

        public IReadOnlyCollection<OrdemServicoServico> Servicos => _servicos.AsReadOnly();
        public IReadOnlyCollection<OrdemServicoInsumo> Insumos => _insumos.AsReadOnly();

        public decimal ValorTotal =>
            _servicos.Sum(s => s.ValorCobrado) +
            _insumos.Sum(p => p.ValorTotal);

        protected OrdemServico() { }

        public OrdemServico(Guid clienteId, Guid veiculoId, string? observacao) : base()
        {
            if (clienteId == Guid.Empty)
                throw new InvalidOrdemServicoException("Cliente é obrigatório.");

            if (veiculoId == Guid.Empty)
                throw new InvalidOrdemServicoException("Veículo é obrigatório.");

            ClienteId = clienteId;
            VeiculoId = veiculoId;
            Observacao = observacao;
            Status = EStatusOrdemServico.Recebida;
            CriadoEm = DateTime.UtcNow;
        }

        public void AdicionarServico(OrdemServicoServico servico)
        {
            if (Status != EStatusOrdemServico.Recebida && Status != EStatusOrdemServico.EmDiagnostico)
                throw new InvalidOrdemServicoException("Serviços só podem ser adicionados quando a Ordem de Serviço estiver recebida ou em diagnóstico.");

            _servicos.Add(servico);
        }

        public void AdicionarInsumo(OrdemServicoInsumo insumo)
        {
            if (Status != EStatusOrdemServico.Recebida && Status != EStatusOrdemServico.EmDiagnostico)
                throw new InvalidOrdemServicoException("Insumos só podem ser adicionados quando a Ordem de Serviço estiver recebida ou em diagnóstico.");

            if (insumo.Origem == EOrigemInsumo.Estoque)
            {
                var existente = _insumos.FirstOrDefault(p => p.InsumoId == insumo.InsumoId);
                if (existente is not null)
                {
                    existente.AdicionarQuantidade(insumo.Quantidade);
                    return;
                }
            }

            _insumos.Add(insumo);
        }

        public void IniciarDiagnostico()
        {
            if (Status != EStatusOrdemServico.Recebida)
                throw new InvalidOrdemServicoException("A Ordem de Serviço só pode ir para diagnóstico quando estiver recebida.");

            Status = EStatusOrdemServico.EmDiagnostico;
        }

        public void AguardarAprovacao()
        {
            if (Status != EStatusOrdemServico.EmDiagnostico)
                throw new InvalidOrdemServicoException("A Ordem de Serviço só pode aguardar aprovação após o diagnóstico.");

            if (!_servicos.Any())
                throw new InvalidOrdemServicoException("A Ordem de Serviço deve ter pelo menos um serviço para aguardar aprovação.");

            Status = EStatusOrdemServico.AguardandoAprovacao;
        }

        public void Aprovar()
        {
            if (Status != EStatusOrdemServico.AguardandoAprovacao)
                throw new InvalidOrdemServicoException("A Ordem de Serviço só pode ser aprovada quando estiver aguardando aprovação.");

            Status = EStatusOrdemServico.EmExecucao;
            IniciadoEm = DateTime.UtcNow;
        }

        public void Recusar()
        {
            if (Status != EStatusOrdemServico.AguardandoAprovacao)
                throw new InvalidOrdemServicoException("A Ordem de Serviço só pode ser recusada quando estiver aguardando aprovação.");

            Status = EStatusOrdemServico.EmDiagnostico;
            IniciadoEm = DateTime.UtcNow;
        }

        public void IniciarServico(Guid ordemServicoServicoId)
        {
            if (Status != EStatusOrdemServico.EmExecucao)
                throw new InvalidOrdemServicoException("A Ordem de Serviço deve estar em execução para iniciar serviços.");

            var servico = _servicos.FirstOrDefault(s => s.Id == ordemServicoServicoId)
                ?? throw new NotFoundException("Serviço não encontrado na Ordem de Serviço.");

            servico.Iniciar();
        }

        public void ConcluirServico(Guid ordemServicoServicoId)
        {
            if (Status != EStatusOrdemServico.EmExecucao)
                throw new InvalidOrdemServicoException("A Ordem de Serviço deve estar em execução para concluir serviços.");

            var servico = _servicos.FirstOrDefault(s => s.Id == ordemServicoServicoId)
                ?? throw new NotFoundException("Serviço não encontrado na Ordem de Serviço.");

            servico.Concluir();

            if (_servicos.All(s => s.Status == EStatusServicoOS.Concluido))
            {
                Status = EStatusOrdemServico.Finalizada;
                FinalizadoEm = DateTime.UtcNow;
            }
        }

        public void Entregar()
        {
            if (Status != EStatusOrdemServico.Finalizada)
                throw new InvalidOrdemServicoException("A Ordem de Serviço só pode ser entregue quando estiver finalizada.");

            Status = EStatusOrdemServico.Entregue;
            EntregueEm = DateTime.UtcNow;
        }
    }
}
