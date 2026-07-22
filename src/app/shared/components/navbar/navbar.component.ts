import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../features/auth/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <header class="main-header">
      <div class="nav-container">
        <!-- Logo & Marca FIAP -->
        <a routerLink="/" class="brand-link">
          <div class="brand-icon">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round">
              <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"></path>
            </svg>
          </div>
          <div class="brand-text">
            <span class="title">AutoReparos</span>
            <span class="subtitle">Oficina Mecânica</span>
          </div>
        </a>

        <!-- Links de Navegação -->
        @if (auth.currentUser()) {
          <nav class="nav-menu">
            <a routerLink="/ordens-servico/fila" routerLinkActive="active" class="nav-item">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M9 3v18"/><path d="M15 3v18"/></svg>
              Fila Kanban
            </a>
            <a routerLink="/ordens-servico" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}" class="nav-item">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>
              Ordens de Serviço
            </a>
            <a routerLink="/clientes" routerLinkActive="active" class="nav-item">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
              Clientes
            </a>
            <a routerLink="/veiculos" routerLinkActive="active" class="nav-item">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M19 17h2c.6 0 1-.4 1-1v-3c0-.9-.7-1.7-1.5-1.9C18.7 10.6 16 10 16 10s-1.3-1.4-2.2-2.3c-.5-.4-1.1-.7-1.8-.7H5c-.6 0-1.1.4-1.4.9l-1.5 3C2 11.3 2 11.7 2 12v4c0 .6.4 1 1 1h2"/><circle cx="7" cy="17" r="2"/><circle cx="17" cy="17" r="2"/></svg>
              Veículos
            </a>

            @if (auth.hasRole(['Administrador'])) {
              <a routerLink="/dashboard" routerLinkActive="active" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                Dashboard
              </a>
              <a routerLink="/insumos" routerLinkActive="active" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/></svg>
                Estoque
              </a>
              <a routerLink="/servicos" routerLinkActive="active" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>
                Serviços
              </a>
              <a routerLink="/usuarios" routerLinkActive="active" class="nav-item">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="8.5" cy="7" r="4"/><line x1="20" y1="8" x2="20" y2="14"/><line x1="23" y1="11" x2="17" y2="11"/></svg>
                Usuários
              </a>
            }
          </nav>
        } @else {
          <div class="nav-menu">
            <a routerLink="/consulta-publica" routerLinkActive="active" class="nav-item">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
              Consultar Status do Veículo
            </a>
          </div>
        }

        <!-- Perfil do Usuário & Ações -->
        <div class="user-section">
          @if (auth.currentUser(); as user) {
            <div class="user-badge">
              <span class="user-name">{{ user.nome }}</span>
              <span class="role-tag">{{ user.role }}</span>
            </div>
            <button (click)="auth.logout()" class="btn-logout" title="Sair do Sistema">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
            </button>
          } @else {
            <a routerLink="/login" class="btn btn-primary btn-sm">Acesso Interno</a>
          }
        </div>
      </div>
    </header>
  `,
  styles: [`
    .main-header {
      background: rgba(24, 24, 28, 0.9);
      backdrop-filter: blur(14px);
      border-bottom: 1px solid rgba(237, 20, 91, 0.25);
      position: sticky;
      top: 0;
      z-index: 100;
      padding: 0.75rem 1.5rem;
    }
    .nav-container {
      max-width: 1280px;
      margin: 0 auto;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1.5rem;
    }
    .brand-link {
      display: flex;
      align-items: center;
      gap: 0.85rem;
    }
    .brand-icon {
      width: 42px;
      height: 42px;
      background: linear-gradient(135deg, #ED145B, #800A30);
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #fff;
      box-shadow: 0 0 15px rgba(237, 20, 91, 0.4);
    }
    .brand-icon svg { width: 24px; height: 24px; }
    .brand-text { display: flex; flex-direction: column; }
    .brand-text .title {
      font-family: 'Outfit', sans-serif;
      font-weight: 800;
      font-size: 1.25rem;
      color: #ffffff;
      line-height: 1.1;
    }
    .brand-text .subtitle {
      font-size: 0.75rem;
      color: #A1A1AA;
    }
    .nav-menu {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .nav-item {
      display: flex;
      align-items: center;
      gap: 0.4rem;
      padding: 0.5rem 0.85rem;
      border-radius: 6px;
      color: #A1A1AA;
      font-size: 0.85rem;
      font-weight: 500;
      transition: all 0.2s ease;
    }
    .nav-item:hover {
      color: #ffffff;
      background: rgba(255, 255, 255, 0.05);
    }
    .nav-item.active {
      color: #ED145B;
      background: rgba(237, 20, 91, 0.12);
      font-weight: 600;
    }
    .user-section {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .user-badge {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
    }
    .user-name {
      font-size: 0.85rem;
      font-weight: 600;
      color: #F8FAFC;
    }
    .role-tag {
      font-size: 0.7rem;
      color: #ED145B;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .btn-logout {
      background: transparent;
      border: 1px solid rgba(255, 255, 255, 0.1);
      color: #A1A1AA;
      padding: 0.4rem 0.6rem;
      border-radius: 6px;
      cursor: pointer;
      display: flex;
      align-items: center;
      transition: all 0.2s ease;
    }
    .btn-logout:hover {
      color: #EF4444;
      border-color: #EF4444;
      background: rgba(239, 68, 68, 0.1);
    }
  `]
})
export class NavbarComponent {
  auth = inject(AuthService);
}
