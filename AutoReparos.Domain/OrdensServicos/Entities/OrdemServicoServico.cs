using AutoReparos.Domain.OrdensServicos.Enums;
using AutoReparos.Domain.OrdensServicos.Exceptions;
using AutoReparos.Domain.Servicos.Entities;
using AutoReparos.Domain.Shared;

namespace AutoReparos.Domain.OrdensServicos.Entities
{
    public class OrdemServicoServico : Entity
    {
        public Guid OrdemServicoId { get; private set; }
        public Guid ServicoId { get; private set; }
        public Servico? Servico { get; private set; }
        public decimal ValorCobrado { get; private set; }
        public EStatusServicoOS Status { get; private set; }
        public DateTime? IniciadoEm { get; private set; }
        public DateTime? ConcluidoEm { get; private set; }

        public TimeSpan? TempoExecucao =>
            IniciadoEm.HasValue && ConcluidoEm.HasValue
                ? ConcluidoEm - IniciadoEm
                : null;

        protected OrdemServicoServico() { }

        public OrdemServicoServico(Guid ordemServicoId, Guid servicoId, decimal valorCobrado) : base()
        {
            if (valorCobrado <= 0)
                throw new InvalidOrdemServicoException("Valor cobrado deve ser maior que zero.");

            OrdemServicoId = ordemServicoId;
            ServicoId = servicoId;
            ValorCobrado = valorCobrado;
            Status = EStatusServicoOS.Pendente;
        }

        public void Iniciar()
        {
            if (Status != EStatusServicoOS.Pendente)
                throw new InvalidOrdemServicoException("Serviço só pode ser iniciado quando estiver pendente.");

            Status = EStatusServicoOS.EmExecucao;
            IniciadoEm = DateTime.UtcNow;
        }

        public void Concluir()
        {
            if (Status != EStatusServicoOS.EmExecucao)
                throw new InvalidOrdemServicoException("Serviço só pode ser concluído quando estiver em execução.");

            Status = EStatusServicoOS.Concluido;
            ConcluidoEm = DateTime.UtcNow;
        }
    }
}
