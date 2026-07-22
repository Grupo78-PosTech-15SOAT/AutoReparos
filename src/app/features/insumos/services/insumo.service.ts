import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Insumo } from '../models/insumo.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';

@Injectable({
  providedIn: 'root'
})
export class InsumoService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<Insumo[]> {
    return this.http.get<Insumo[]>(API_ENDPOINTS.INSUMOS.BASE);
  }

  getById(id: string): Observable<Insumo> {
    return this.http.get<Insumo>(API_ENDPOINTS.INSUMOS.BY_ID(id));
  }

  criar(insumo: Insumo): Observable<Insumo> {
    return this.http.post<Insumo>(API_ENDPOINTS.INSUMOS.BASE, insumo);
  }

  atualizar(id: string, insumo: Insumo): Observable<Insumo> {
    return this.http.put<Insumo>(API_ENDPOINTS.INSUMOS.BY_ID(id), insumo);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(API_ENDPOINTS.INSUMOS.BY_ID(id));
  }
}
