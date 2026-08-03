using System;
using System.Collections.Generic;
using AutoReparos.Application.OrdensServicos.DTOs.Response;
using AutoReparos.Domain.OrdensServicos.Entities;
using AutoReparos.Domain.OrdensServicos.Enums;

namespace AutoReparos.Application.OrdensServicos.Strategies.Interfaces
{
    public interface IKanbanCardStrategy
    {
        bool CanHandle(EStatusOrdemServico status);
        string ColumnKey { get; }
        KanbanCardDto CreateCard(OrdemServico os, IDictionary<string, string> usuarioDict);
    }
}
