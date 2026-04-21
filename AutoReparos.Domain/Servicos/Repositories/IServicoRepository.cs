using AutoReparos.Domain.Servicos.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoReparos.Domain.Servicos.Repositories
{
    public interface IServicoRepository
    {
        Task Create(Servico servico);
        Task<Servico?> GetById(Guid id);
        Task<(IEnumerable<Servico> Items, int Total)> GetAll(string? nome, int skip, int take);
        Task<IEnumerable<(Guid ServicoId, string NomeServico, TimeSpan TempoMedio, int TotalExecucoes)>> GetTempoMedio();
        Task<(Guid ServicoId, string NomeServico, TimeSpan TempoMedio, int TotalExecucoes)?> GetTempoMedioById(Guid id);
        Task Update(Servico servico);
        Task Delete(Servico servico);
    }
}
