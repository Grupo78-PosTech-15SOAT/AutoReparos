import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Servico } from '../models/servico.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';

@Injectable({
  providedIn: 'root'
})
export class ServicoService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<Servico[]> {
    return this.http.get<Servico[]>(API_ENDPOINTS.SERVICOS.BASE);
  }

  getById(id: string): Observable<Servico> {
    return this.http.get<Servico>(API_ENDPOINTS.SERVICOS.BY_ID(id));
  }

  criar(servico: Servico): Observable<Servico> {
    return this.http.post<Servico>(API_ENDPOINTS.SERVICOS.BASE, servico);
  }

  atualizar(id: string, servico: Servico): Observable<Servico> {
    return this.http.put<Servico>(API_ENDPOINTS.SERVICOS.BY_ID(id), servico);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(API_ENDPOINTS.SERVICOS.BY_ID(id));
  }
}
