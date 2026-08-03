import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Veiculo } from '../models/veiculo.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { PagedResult } from '../../../shared/models/pagination.model';

@Injectable({
  providedIn: 'root'
})
export class VeiculoService {
  private readonly http = inject(HttpClient);

  getAll(pageNumber = 1, pageSize = 10): Observable<PagedResult<Veiculo>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<any>(API_ENDPOINTS.VEICULOS.BASE, { params }).pipe(
      map(res => this.normalizePagedResult(res, pageNumber, pageSize))
    );
  }

  getById(id: string): Observable<Veiculo> {
    return this.http.get<Veiculo>(API_ENDPOINTS.VEICULOS.BY_ID(id));
  }

  buscarPorPlaca(placa: string): Observable<Veiculo> {
    return this.http.get<Veiculo>(API_ENDPOINTS.VEICULOS.BY_PLACA(placa));
  }

  criar(veiculo: Veiculo): Observable<Veiculo> {
    return this.http.post<Veiculo>(API_ENDPOINTS.VEICULOS.BASE, veiculo);
  }

  atualizar(id: string, veiculo: Veiculo): Observable<Veiculo> {
    return this.http.put<Veiculo>(API_ENDPOINTS.VEICULOS.BY_ID(id), veiculo);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(API_ENDPOINTS.VEICULOS.BY_ID(id));
  }

  private normalizePagedResult(res: any, pageNumber: number, pageSize: number): PagedResult<Veiculo> {
    if (res) {
      const items = res.items ?? res.Items ?? res.data ?? res.Data ?? (Array.isArray(res) ? res : null);
      if (Array.isArray(items)) {
        const total = res.totalItems ?? res.TotalItems ?? res.total ?? res.Total ?? items.length;
        const pNum = res.pageNumber ?? res.PageNumber ?? pageNumber ?? 1;
        const pSize = res.pageSize ?? res.PageSize ?? pageSize ?? 10;
        const totalPages = res.totalPages ?? res.TotalPages ?? Math.max(1, Math.ceil(total / (pSize || 1)));
        return { items, total, pageNumber: pNum, pageSize: pSize, totalPages };
      }
    }
    return { items: [], total: 0, pageNumber: pageNumber || 1, pageSize: pageSize || 10, totalPages: 1 };
  }
}
