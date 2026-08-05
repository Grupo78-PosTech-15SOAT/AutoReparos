import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Usuario } from '../models/usuario.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { PagedResult } from '../../../shared/models/pagination.model';

@Injectable({
  providedIn: 'root'
})
export class UsuarioService {
  private readonly http = inject(HttpClient);

  getAll(pageNumber = 1, pageSize = 10): Observable<PagedResult<Usuario>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<any>(API_ENDPOINTS.USUARIOS.BASE, { params }).pipe(
      map(res => this.normalizePagedResult(res, pageNumber, pageSize))
    );
  }

  criar(usuario: Usuario): Observable<Usuario> {
    const roleMapping: Record<string, number> = {
      'Administrador': 1,
      'Atendente': 2,
      'Mecanico': 3
    };

    const payload = {
      nomeCompleto: usuario.nome,
      email: usuario.email,
      password: usuario.senha,
      tipo: roleMapping[usuario.role] || 3
    };

    return this.http.post<Usuario>(API_ENDPOINTS.USUARIOS.BASE, payload);
  }

  atualizarRole(id: string, role: string): Observable<Usuario> {
    return this.http.patch<Usuario>(API_ENDPOINTS.USUARIOS.ROLE(id), { role });
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(API_ENDPOINTS.USUARIOS.BY_ID(id));
  }

  private normalizePagedResult(res: any, pageNumber: number, pageSize: number): PagedResult<Usuario> {
    if (res) {
      let items = res.items ?? res.Items ?? res.data ?? res.Data ?? (Array.isArray(res) ? res : null);
      if (Array.isArray(items)) {
        const roleReverseMapping: Record<number, string> = {
          1: 'Administrador',
          2: 'Atendente',
          3: 'Mecanico'
        };
        items = items.map((u: any) => ({
          ...u,
          nome: u.nome ?? u.nomeCompleto ?? u.NomeCompleto ?? '',
          role: u.role ?? roleReverseMapping[u.tipo] ?? roleReverseMapping[u.Tipo] ?? 'Mecanico'
        }));
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
