export interface Usuario {
  id?: string;
  nome: string;
  email: string;
  role: 'Administrador' | 'Atendente' | 'Mecanico' | 'Cliente';
  tipo?: string;
  senha?: string;
  ativo?: boolean;
}
