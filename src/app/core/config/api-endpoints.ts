import { environment } from '../../../environments/environment';

export const API_BASE_URL = environment.apiUrl;

export const API_ENDPOINTS = {
  // 1. Autenticação & Login
  AUTH: {
    LOGIN: `${API_BASE_URL}/api/v1/auth/login`,
  },

  // 2. Ordens de Serviço & Sub-endpoints
  ORDENS_SERVICO: {
    BASE: `${API_BASE_URL}/api/v1/ordensservico`,
    FILA_KANBAN: `${API_BASE_URL}/api/v1/ordensservico/fila-kanban`,
    CONSULTA_PUBLICA: `${API_BASE_URL}/api/v1/ordensservico/consulta-publica`,
    ENVIAR_APROVACAO: (id: string) => `${API_BASE_URL}/api/v1/ordensservico/${id}/enviar-aprovacao`,
    RESPONDER_ORCAMENTO: `${API_BASE_URL}/api/v1/ordensservico/aprovar-orcamento`,
    ENTREGAR: (id: string) => `${API_BASE_URL}/api/v1/ordensservico/${id}/entregar`,
    STATUS: (id: string) => `${API_BASE_URL}/api/v1/ordensservico/${id}/status`,
    DIAGNOSTICO: (id: string) => `${API_BASE_URL}/api/v1/ordensservico/${id}/diagnostico`,
    SERVICOS: (id: string) => `${API_BASE_URL}/api/v1/ordensservico/${id}/servicos`,
    SERVICO_STATUS: (osId: string, itemId: string) => `${API_BASE_URL}/api/v1/ordensservico/${osId}/servicos/${itemId}/status`,
    INSUMOS: (osId: string) => `${API_BASE_URL}/api/v1/ordensservico/${osId}/insumos`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/v1/ordensservico/${id}`
  },

  // 3. Gestão de Clientes & Sub-endpoints
  CLIENTES: {
    BASE: `${API_BASE_URL}/api/v1/clientes`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/v1/clientes/${id}`
  },

  // 4. Gestão de Veículos & Sub-endpoints
  VEICULOS: {
    BASE: `${API_BASE_URL}/api/v1/veiculos`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/v1/veiculos/${id}`,
    BY_PLACA: (placa: string) => `${API_BASE_URL}/api/v1/veiculos/placa/${placa}`
  },

  // 5. Gestão de Insumos / Peças & Sub-endpoints
  INSUMOS: {
    BASE: `${API_BASE_URL}/api/v1/insumos`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/v1/insumos/${id}`
  },

  // 6. Catálogo de Serviços & Sub-endpoints
  SERVICOS: {
    BASE: `${API_BASE_URL}/api/v1/servicos`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/v1/servicos/${id}`
  },

  // 7. Gestão de Usuários & Roles
  USUARIOS: {
    BASE: `${API_BASE_URL}/api/v1/usuarios`,
    BY_ID: (id: string) => `${API_BASE_URL}/api/v1/usuarios/${id}`,
    ROLE: (id: string) => `${API_BASE_URL}/api/v1/usuarios/${id}/role`
  },

  // 8. Dashboard Gerencial
  DASHBOARD: {
    METRICS: `${API_BASE_URL}/api/v1/dashboard/metrics`
  }
};
