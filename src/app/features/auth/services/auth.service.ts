import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, UserTokenInfo } from '../models/auth.model';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  currentUser = signal<UserTokenInfo | null>(this.loadStoredUser());

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(API_ENDPOINTS.AUTH.LOGIN, credentials).pipe(
      tap(response => {
        if (response && response.token) {
          localStorage.setItem('autoreparos_token', response.token);
          const user: UserTokenInfo = response.usuario ? {
            ...response.usuario,
            nome: response.usuario.nome || response.usuario.nomeCompleto || response.usuario.email.split('@')[0],
            role: response.usuario.role || 'Administrador'
          } : {
            email: response.email,
            nomeCompleto: response.nomeCompleto || response.nome || response.email,
            nome: response.nome || response.nomeCompleto || (response.email ? response.email.split('@')[0] : 'Usuário'),
            role: response.role || 'Administrador'
          };
          localStorage.setItem('autoreparos_user', JSON.stringify(user));
          this.currentUser.set(user);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('autoreparos_token');
    localStorage.removeItem('autoreparos_user');
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('autoreparos_token');
  }

  hasRole(allowedRoles: string[]): boolean {
    return this.isAuthenticated();
  }

  private loadStoredUser(): UserTokenInfo | null {
    const userJson = localStorage.getItem('autoreparos_user');
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
