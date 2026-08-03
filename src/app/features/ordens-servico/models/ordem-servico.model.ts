export enum StatusOS {
  Recebida = 1,
  EmDiagnostico = 2,
  AguardandoAprovacao = 3,
  EmExecucao = 4,
  Finalizada = 5,
  Entregue = 6
}

export interface ItemServicoOS {
  id?: string;
  servicoId: string;
  nomeServico: string;
  valor: number;
  concluido?: boolean;
}

export interface ItemInsumoOS {
  id?: string;
  insumoId: string;
  nomeInsumo: string;
  quantidade: number;
  valorUnitario: number;
  valorTotal: number;
}

export interface ServicoDetalhe {
  id: string;
  servicoId: string;
  nomeServico?: string;
  valorCobrado: number;
  status: string;         // "Pendente" | "EmExecucao" | "Concluido"
  iniciadoEm?: string;
  concluidoEm?: string;
}

export interface InsumoDetalhe {
  id: string;
  insumoId?: string;
  descricao: string;
  valorUnitario: number;
  quantidade: number;
  valorTotal: number;
  origem: string;         // "Estoque" | "CompraEspecifica"
}

export interface OsDetalhe {
  id: string;
  clienteId: string;
  clienteNome?: string;
  veiculoId: string;
  placaVeiculo?: string;
  modeloVeiculo?: string;
  status: StatusOS | string | number;
  observacao?: string;
  valorTotal: number;
  criadoEm: string;
  iniciadoEm?: string;
  finalizadoEm?: string;
  entregueEm?: string;
  envioAprovacaoEm?: string;
  responsavelId?: string;
  responsavelNome?: string;
  servicos: ServicoDetalhe[];
  insumos: InsumoDetalhe[];
}

export interface OrdemServico {
  id: string;
  clienteId: string;
  clienteNome: string;
  clienteDocumento?: string;
  veiculoId: string;
  placaVeiculo: string;
  modeloVeiculo: string;
  status: StatusOS;
  dataAbertura: string;
  dataPrevisao?: string;
  dataFinalizacao?: string;
  observacoesDiagnostico?: string;
  valorTotal: number;
  itensServico: ItemServicoOS[];
  itensInsumo: ItemInsumoOS[];
  approvalToken?: string;
}

export interface CriarOSRequest {
  clienteId: string;
  veiculoId: string;
  observacoesIniciais?: string;
}

export interface AdicionarServicoOSRequest {
  servicoId: string;
}

export interface AdicionarInsumoOSRequest {
  insumoId: string;
  quantidade: number;
}

export interface KanbanCard {
  $type: string;
  id: string;
  clienteNome: string;
  placaVeiculo: string;
  modeloVeiculo: string;
  status: string;
  dataEntrada?: string;
  mecanicoResponsavel?: string;
  tempoDiagnostico?: string;
  valorOrcamento?: number;
  quantidadeServicos?: number;
  tempoAguardandoAprovacao?: string;
  valorAprovado?: number;
  progressoServicos?: number;
  valorFinal?: number;
  dataConclusao?: string;
  statusEntrega?: string;
  servicosConcluidos?: number;
  servicosTotal?: number;
}

export interface KanbanColumn {
  status: string;
  cards: KanbanCard[];
}
