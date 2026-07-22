import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';

export interface DashboardMetrics {
  totalOSMes: number;
  faturamentoEstimado: number;
  ordensEmExecucao: number;
  alertasEstoqueMinimo: number;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(private http: HttpClient) {}

  getMetrics(): Observable<DashboardMetrics> {
    return this.http.get<DashboardMetrics>(API_ENDPOINTS.DASHBOARD.METRICS);
  }
}
