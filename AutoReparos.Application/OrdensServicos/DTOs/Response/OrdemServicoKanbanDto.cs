using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AutoReparos.Application.OrdensServicos.DTOs.Response
{
    public record KanbanColumnDto(string Status, IEnumerable<KanbanCardDto> Cards);

    [JsonDerivedType(typeof(ReceivedKanbanCardDto), "Received")]
    [JsonDerivedType(typeof(DiagnosisKanbanCardDto), "Diagnosis")]
    [JsonDerivedType(typeof(ApprovalKanbanCardDto), "Approval")]
    [JsonDerivedType(typeof(ExecutionKanbanCardDto), "Execution")]
    [JsonDerivedType(typeof(FinishedKanbanCardDto), "Finished")]
    public abstract record KanbanCardDto;

    public record ReceivedKanbanCardDto(
        Guid Id, 
        string NumeroOS, 
        string ClienteNome, 
        string PlacaVeiculo, 
        string ModeloVeiculo, 
        string Status, 
        DateTime DataEntrada) : KanbanCardDto;

    public record DiagnosisKanbanCardDto(
        Guid Id, 
        string NumeroOS, 
        string ClienteNome, 
        string PlacaVeiculo, 
        string ModeloVeiculo, 
        string Status, 
        string? ResponsavelId) : KanbanCardDto;

    public record ApprovalKanbanCardDto(
        Guid Id, 
        string NumeroOS, 
        string ClienteNome, 
        string PlacaVeiculo, 
        string ModeloVeiculo, 
        string Status, 
        decimal ValorOrcamento, 
        int QuantidadeServicos, 
        DateTime? EnvioAprovacaoEm) : KanbanCardDto;

    public record ExecutionKanbanCardDto(
        Guid Id, 
        string NumeroOS, 
        string ClienteNome, 
        string PlacaVeiculo, 
        string ModeloVeiculo, 
        string Status, 
        decimal ValorAprovado, 
        double ProgressoServicos, 
        string? MecanicoResponsavel,
        int ServicosConcluidos,
        int ServicosTotal) : KanbanCardDto;

    public record FinishedKanbanCardDto(
        Guid Id, 
        string NumeroOS, 
        string ClienteNome, 
        string PlacaVeiculo, 
        string ModeloVeiculo, 
        string Status, 
        decimal ValorFinal, 
        DateTime DataConclusao, 
        string StatusEntrega) : KanbanCardDto;
}
