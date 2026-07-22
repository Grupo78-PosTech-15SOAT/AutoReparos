import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Usuario } from '../models/usuario.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';

@Injectable({
  providedIn: 'root'
})
export class UsuarioService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>(API_ENDPOINTS.USUARIOS.BASE);
  }

  criar(usuario: Usuario): Observable<Usuario> {
    return this.http.post<Usuario>(API_ENDPOINTS.USUARIOS.BASE, usuario);
  }

  atualizarRole(id: string, role: string): Observable<Usuario> {
    return this.http.patch<Usuario>(API_ENDPOINTS.USUARIOS.ROLE(id), { role });
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(API_ENDPOINTS.USUARIOS.BY_ID(id));
  }
}
