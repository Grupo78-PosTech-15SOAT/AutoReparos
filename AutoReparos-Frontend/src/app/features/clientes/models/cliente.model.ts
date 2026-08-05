export interface Cliente {
  id?: string;
  nome: string;
  email: string;
  telefone: string;
  documento: string; // CPF ou CNPJ
  tipoDocumento: 'CPF' | 'CNPJ';
  endereco?: string;
  dataCadastro?: string;
}
