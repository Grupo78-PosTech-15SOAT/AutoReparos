import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Veiculo } from '../models/veiculo.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';

@Injectable({
  providedIn: 'root'
})
export class VeiculoService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<Veiculo[]> {
    return this.http.get<Veiculo[]>(API_ENDPOINTS.VEICULOS.BASE);
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
}
