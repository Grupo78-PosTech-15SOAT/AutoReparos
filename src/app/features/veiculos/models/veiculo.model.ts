export interface Veiculo {
  id?: string;
  placa: string;
  marca: string;
  modelo: string;
  ano: number;
  cor?: string;
  clienteId: string;
  clienteNome?: string;
  dataCadastro?: string;
}
