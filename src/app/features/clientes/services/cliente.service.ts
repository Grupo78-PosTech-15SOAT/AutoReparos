import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Cliente } from '../models/cliente.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';

@Injectable({
  providedIn: 'root'
})
export class ClienteService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<Cliente[]> {
    return this.http.get<Cliente[]>(API_ENDPOINTS.CLIENTES.BASE);
  }

  getById(id: string): Observable<Cliente> {
    return this.http.get<Cliente>(API_ENDPOINTS.CLIENTES.BY_ID(id));
  }

  criar(cliente: Cliente): Observable<Cliente> {
    return this.http.post<Cliente>(API_ENDPOINTS.CLIENTES.BASE, cliente);
  }

  atualizar(id: string, cliente: Cliente): Observable<Cliente> {
    return this.http.put<Cliente>(API_ENDPOINTS.CLIENTES.BY_ID(id), cliente);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(API_ENDPOINTS.CLIENTES.BY_ID(id));
  }
}
