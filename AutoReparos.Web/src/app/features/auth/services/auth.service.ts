import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, UserTokenInfo } from '../models/auth.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { STORAGE_TOKEN } from '../../../core/tokens/storage.token';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storage = inject(STORAGE_TOKEN);

  currentUser = signal<UserTokenInfo | null>(this.loadStoredUser());

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(API_ENDPOINTS.AUTH.LOGIN, credentials).pipe(
      tap(response => {
        if (response?.token) {
          this.storage.setItem('autoreparos_token', response.token);
          const user: UserTokenInfo = response.usuario ? {
            ...response.usuario,
            nome: response.usuario.nome || response.usuario.nomeCompleto || response.usuario.email.split('@')[0],
            role: response.usuario.role || 'Usuário'
          } : {
            email: response.email,
            nomeCompleto: response.nomeCompleto || response.nome || response.email,
            nome: response.nome || response.nomeCompleto || (response.email ? response.email.split('@')[0] : 'Usuário'),
            role: response.role || 'Usuário'
          };
          this.storage.setItem('autoreparos_user', JSON.stringify(user));
          this.currentUser.set(user);
        }
      })
    );
  }

  logout(): void {
    this.storage.removeItem('autoreparos_token');
    this.storage.removeItem('autoreparos_user');
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return !!this.storage.getItem('autoreparos_token');
  }

  hasRole(allowedRoles: string[]): boolean {
    const user = this.currentUser();
    return !!user && !!user.role && allowedRoles.includes(user.role);
  }

  private loadStoredUser(): UserTokenInfo | null {
    const userJson = this.storage.getItem('autoreparos_user');
    if (userJson) {
      try {
        return JSON.parse(userJson);
      } catch {
        return null;
      }
    }
    return null;
  }
}
