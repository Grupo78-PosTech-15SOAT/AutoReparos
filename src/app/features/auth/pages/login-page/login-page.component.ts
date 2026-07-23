import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../../../core/ui/notification.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="login-container fade-in">
      <div class="login-card">
        <!-- Header -->
        <div class="brand-header">
          <div class="brand-logo">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round">
              <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"></path>
            </svg>
          </div>
          <h1 class="brand-title">AutoReparos</h1>
          <p class="brand-subtitle">Painel Interno da Oficina</p>
        </div>

        <!-- Formulário -->
        <form (ngSubmit)="onSubmit()" class="login-form">
          <div class="form-group">
            <label class="form-label">E-mail</label>
            <input type="email" [(ngModel)]="email" name="email" required placeholder="seu@email.com" class="form-control" />
          </div>

          <div class="form-group">
            <label class="form-label">Senha</label>
            <input type="password" [(ngModel)]="senha" name="senha" required placeholder="••••••••" class="form-control" />
          </div>

          <button type="submit" [disabled]="loading" class="btn btn-primary btn-block" style="margin-top: 1rem; display: flex; align-items: center; justify-content: center; gap: 0.5rem;">
            @if (loading) {
              <svg class="spinner" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round">
                <path d="M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0"/>
              </svg>
              <span>Autenticando...</span>
            } @else {
              <span>Entrar no Sistema</span>
            }
          </button>
        </form>

        <!-- Perfis Rápidos para Demo -->
        <div class="quick-roles">
          <span class="quick-title">Acesso rápido:</span>
          <div class="quick-buttons">
            <button type="button" (click)="fillEmail('admin@autoreparos.com')" class="btn-role">Admin</button>
            <button type="button" (click)="fillEmail('atendente@autoreparos.com')" class="btn-role">Atendente</button>
            <button type="button" (click)="fillEmail('mecanico@autoreparos.com')" class="btn-role">Mecânico</button>
          </div>
        </div>

        <div class="login-footer">
          <a routerLink="/consulta-publica" class="public-link">Página Pública do Cliente &rarr;</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      min-height: 80vh;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem 1rem;
    }
    .login-card {
      background: rgba(24, 24, 28, 0.9);
      backdrop-filter: blur(16px);
      border: 1px solid rgba(237, 20, 91, 0.3);
      border-radius: 16px;
      padding: 2.5rem;
      max-width: 440px;
      width: 100%;
      box-shadow: 0 20px 40px rgba(0, 0, 0, 0.8);
    }
    .brand-header {
      text-align: center;
      margin-bottom: 2rem;
    }
    .brand-logo {
      width: 58px;
      height: 58px;
      background: linear-gradient(135deg, #ED145B, #800A30);
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #ffffff;
      margin: 0 auto 1rem;
      box-shadow: 0 0 25px rgba(237, 20, 91, 0.4);
    }
    .brand-logo svg { width: 32px; height: 32px; }
    .brand-title {
      font-family: 'Outfit', sans-serif;
      font-size: 2rem;
      font-weight: 800;
      color: #ffffff;
    }
    .brand-subtitle {
      font-size: 0.875rem;
      color: #A1A1AA;
    }
    .btn-block { width: 100%; }
    .quick-roles {
      margin-top: 2rem;
      padding-top: 1.5rem;
      border-top: 1px solid rgba(255, 255, 255, 0.08);
      text-align: center;
    }
    .quick-title {
      font-size: 0.75rem;
      color: #71717A;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      display: block;
      margin-bottom: 0.75rem;
    }
    .quick-buttons {
      display: flex;
      gap: 0.5rem;
      justify-content: center;
    }
    .btn-role {
      background: rgba(255, 255, 255, 0.06);
      border: 1px solid rgba(255, 255, 255, 0.12);
      color: #E2E8F0;
      padding: 0.35rem 0.75rem;
      border-radius: 6px;
      font-size: 0.75rem;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s ease;
    }
    .btn-role:hover {
      background: rgba(237, 20, 91, 0.2);
      border-color: #ED145B;
      color: #ED145B;
    }
    .login-footer {
      margin-top: 1.5rem;
      text-align: center;
    }
    .public-link {
      font-size: 0.85rem;
      color: #A1A1AA;
      transition: color 0.2s;
    }
    .public-link:hover { color: #ED145B; }
    .spinner {
      animation: spin 0.8s linear infinite;
    }
    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }
  `]
})
export class LoginPageComponent {
  email = '';
  senha = '';
  loading = false;

  private authService = inject(AuthService);
  private notification = inject(NotificationService);
  private router = inject(Router);

  fillEmail(e: string) {
    this.email = e;
    this.senha = '';
  }

  onSubmit() {
    if (!this.email || !this.senha) {
      this.notification.warning('Campos Obrigatórios', 'Preencha o e-mail e a senha.');
      return;
    }

    this.loading = true;
    this.authService.login({ email: this.email, password: this.senha })
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({
        next: () => {
          this.notification.success('Autenticado com Sucesso', 'Bem-vindo ao AutoReparos!');
          this.router.navigate(['/ordens-servico/fila']);
        },
        error: () => {
          // Desfoca do botão de login e limpa os campos de senha e email para resetar o estado
          (document.activeElement as HTMLElement)?.blur();
          this.senha = '';
        }
      });
  }
}
