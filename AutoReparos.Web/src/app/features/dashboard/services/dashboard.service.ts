import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { StatusOS } from '../../ordens-servico/models/ordem-servico.model';

export interface DashboardMensalStatus {
  mes: string;
  totalOrdens: number;
  totalFaturado: number;
  totalServicosRealizados: number;
}

export interface DashboardMetrics {
  faturamentoMesAtual: number;
  faturamentoMesAnterior: number;
  ordensEmExecucao: number;
  totalOrdensMesAtual: number;
  ultimasOrdens: DashboardOrdemServico[];
  insumosCriticos: DashboardInsumoCritico[];
  historicoMensal: DashboardMensalStatus[];
}

export interface DashboardOrdemServico {
  id: string;
  status: StatusOS;
  valorTotal: number;
  criadoEm: string;
  modeloVeiculo: string;
  placaVeiculo: string;
}

export interface DashboardInsumoCritico {
  id: string;
  nome: string;
  quantidadeEstoque: number;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly http = inject(HttpClient);

  getMetrics(): Observable<DashboardMetrics> {
    return this.http.get<DashboardMetrics>(API_ENDPOINTS.DASHBOARD.METRICS);
  }
}
