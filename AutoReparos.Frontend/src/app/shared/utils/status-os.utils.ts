export interface StatusOsInfo {
  label: string;
  cssClass: string;
}

export const STATUS_OS_MAP: Record<number, StatusOsInfo> = {
  1: { label: 'Recebida',             cssClass: 'status-recebida'    },
  2: { label: 'Em Diagnóstico',       cssClass: 'status-diagnostico' },
  3: { label: 'Aguardando Aprovação', cssClass: 'status-aguardando'  },
  4: { label: 'Em Execução',          cssClass: 'status-execucao'    },
  5: { label: 'Finalizada',           cssClass: 'status-finalizada'  },
  6: { label: 'Entregue',             cssClass: 'status-entregue'    },
};

export function getStatusOsInfo(status: number | string): StatusOsInfo {
  return STATUS_OS_MAP[Number(status)] ?? { label: 'Desconhecido', cssClass: 'status-entregue' };
}
