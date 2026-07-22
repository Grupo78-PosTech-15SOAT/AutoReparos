export interface Usuario {
  id?: string;
  nome: string;
  email: string;
  role: 'Administrador' | 'Atendente' | 'Mecanico' | 'Cliente';
  senha?: string;
  ativo?: boolean;
}
