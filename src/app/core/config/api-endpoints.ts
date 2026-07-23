import { environment } from '../../../environments/environment';

export const API_BASE_URL = environment.apiUrl;

export const API_ENDPOINTS = {
  // 1. Autenticação & Login
  AUTH: {
    LOGIN: `${API_BASE_URL}/api/auth/login`,
  },

  // 2. Ordens de Serviço & Sub-endpoints
  ORDENS_SERVICO: {
    BASE: `${API_BASE_URL}/api/ordem-servico`,
    FILA_KANBAN: `${API_BASE_URL}/api/ordem-servico/fila`,
    CONSULTA_PUBLICA: `${API_BASE_URL}/api/ordem-servico/consulta`,
    ENVIAR_APROVACAO: (id: string) => `${API_BASE_URL}/api/ordem-servico/${id}/enviar-para-aprovacao`,
    RESPONDER_ORCAMENTO: `${API_BASE_URL}/api/ordem-servico/aprovar`,
    APROVAR_ORCAMENTO: (token: string) => `${API_BASE_URL}/api/ordem-servico/aprovar?token=${encodeURIComponent(token)}`,
    RECUSAR_ORCAMENTO: (token: string) => `${API_BASE_URL}/api/ordem-servico/recusar?token=${encodeURIComponent(token)}`,
    ENTREGAR: (id: string) => `${API_BASE_URL}/api/ordem-servico/${id}/entregar`,
    STATUS: (id: string) => `${API_BASE_URL}/api/ordem-servico/${id}/status`,
    DIAGNOSTICO: (id: string) => `${API_BASE_URL}/api/ordem-servico/${id}/iniciar-diagnostico`,
    SERVICOS: (id: string) => `${API_BASE_URL}/api/ordem-servico/${id}/servicos`,
    SERVICO_STATUS: (osId: string, itemId: string) => `${API_BASE_URL}/api/ordem-servico/${osId}/servicos/${itemId}/concluir`,
    INSUMOS: (osId: string) => `${API_BASE_URL}/api/ordem-servico/${osId}/insumos`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/ordem-servico/${id}`
  },

  // 3. Gestão de Clientes
  CLIENTES: {
    BASE: `${API_BASE_URL}/api/clientes`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/clientes/${id}`
  },

  // 4. Gestão de Veículos
  VEICULOS: {
    BASE: `${API_BASE_URL}/api/veiculos`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/veiculos/${id}`,
    BY_PLACA: (placa: string) => `${API_BASE_URL}/api/veiculos/placa/${placa}`
  },

  // 5. Gestão de Insumos / Peças
  INSUMOS: {
    BASE: `${API_BASE_URL}/api/insumos`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/insumos/${id}`
  },

  // 6. Catálogo de Serviços
  SERVICOS: {
    BASE: `${API_BASE_URL}/api/servicos`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/servicos/${id}`
  },

  // 7. Gestão de Usuários
  USUARIOS: {
    BASE: `${API_BASE_URL}/api/usuarios`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/usuarios/${id}`,
    ROLE: (id: string) => `${API_BASE_URL}/api/usuarios/${id}/role`
  },

  // 8. Dashboard Gerencial
  DASHBOARD: {
    METRICS: `${API_BASE_URL}/api/dashboard/metrics`
  }
};
