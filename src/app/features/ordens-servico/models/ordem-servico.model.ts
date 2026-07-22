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

export interface OrdemServico {
  id: string;
  numeroOS: string;
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
