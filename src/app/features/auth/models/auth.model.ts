export interface LoginRequest {
  email: string;
  senha?: string;
  password?: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  nomeCompleto?: string;
  nome?: string;
  role?: string;
  usuario?: UserTokenInfo;
}

export interface UserTokenInfo {
  id?: string;
  email: string;
  nomeCompleto?: string;
  nome?: string;
  role?: string;
}

