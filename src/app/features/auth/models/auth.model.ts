export interface LoginRequest {
  email: string;
  senha: string;
}

export interface LoginResponse {
  token: string;
  usuario: UserTokenInfo;
}

export interface UserTokenInfo {
  id: string;
  nome: string;
  email: string;
  role: 'Administrador' | 'Atendente' | 'Mecanico' | 'Cliente';
}
