export interface Veiculo {
  id?: string;
  placa: string;
  marca: string;
  modelo: string;
  anoFabricacao: number;
  anoModelo: number;
  clienteId: string;
  clienteNome?: string;
  dataCadastro?: string;
}
